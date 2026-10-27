using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Docflow
{
	public class DocflowSendHandler : IDisposable
	{
		private readonly ILogger<DocflowSendHandler> _logger;
		private readonly TransferOrderUpdInfoFactory _transferOrderUpdInfoFactory;
		private readonly OrderUpdInfoFactory _orderUpdInfoFactory;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly IUnitOfWork _uow;
		private readonly IInformalOrderDocumentHandlerFactory _documentHandlerFactory;

		public DocflowSendHandler(
			ILogger<DocflowSendHandler> logger,
			IUnitOfWork uow,
			TransferOrderUpdInfoFactory transferOrderUpdInfoFactory,
			OrderUpdInfoFactory orderUpdInfoFactory,
			IPaymentRepository paymentRepository,
			IEdoRepository edoRepository,
			IInformalOrderDocumentHandlerFactory informalOrderDocumentHandlerFactory,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferOrderUpdInfoFactory = transferOrderUpdInfoFactory ?? throw new ArgumentNullException(nameof(transferOrderUpdInfoFactory));
			_orderUpdInfoFactory = orderUpdInfoFactory ?? throw new ArgumentNullException(nameof(orderUpdInfoFactory));
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_documentHandlerFactory = informalOrderDocumentHandlerFactory ?? throw new ArgumentNullException(nameof(informalOrderDocumentHandlerFactory));
		}

		public async Task SendTransferDocument(int transferDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(transferDocumentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning("Документ {documentId} не найден", transferDocumentId);
				return;
			}

			var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status);
			if(isNotValidStatus)
			{
				_logger.LogError("Документ {documentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);
			if(transferTask is null)
			{
				_logger.LogWarning("Задача для документа {documentId} не найдена", transferDocumentId);
				return;
			}

			var transferOrder = await _uow.Session.QueryOver<TransferOrder>()
				.Where(x => x.Id == transferTask.TransferOrderId)
				.SingleOrDefaultAsync(cancellationToken);

			var updInfo = await _transferOrderUpdInfoFactory.CreateUniversalTransferDocumentInfo(transferOrder, cancellationToken);

			var message = new TaxcomDocflowSendEvent
			{
				EdoAccount = transferOrder.Seller.TaxcomEdoSettings.EdoAccount,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo,
				EdoTaskId = transferTask.Id
			};
			await _publishEndpoint.Publish(message);
		}

		public async Task SendOrderDocument(int orderDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OrderEdoDocument>(orderDocumentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning("Документ {documentId} не найден", orderDocumentId);
				return;
			}

			var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status); 
			if(isNotValidStatus)
			{
				_logger.LogError("Документ {documentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var documentTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);
			if(documentTask is null)
			{
				_logger.LogWarning("Задача для документа {documentId} не найдена", orderDocumentId);
				return;
			}

			UniversalTransferDocumentInfo updInfo;
			OrganizationEntity sender;

			switch(documentTask.FormalEdoRequest.Type)
			{
				case CustomerEdoRequestType.Order:
					var order = documentTask.FormalEdoRequest.Order;
					var payments = _paymentRepository.GetOrderPayments(_uow, order.Id)
						.Select(payment => payment.GetFirstParentRefundedPaymentOrCurrent())
						.Where(x => order.DeliveryDate.HasValue && x.Date < order.DeliveryDate.Value.AddDays(1))
						.Distinct()
						.ToList();
					
					sender = order.Contract.Organization;
					updInfo = await _orderUpdInfoFactory.CreateUniversalTransferDocumentInfo(documentTask, payments, cancellationToken);
					break;
				case CustomerEdoRequestType.OrderWithoutShipmentForAdvancePayment:
				case CustomerEdoRequestType.OrderWithoutShipmentForDebt:
				case CustomerEdoRequestType.OrderWithoutShipmentForPayment:
					throw new NotImplementedException("Не реализована отправка счетов без отгрузки");
				default:
					throw new InvalidOperationException($"Неизвестный тип заявки {documentTask.FormalEdoRequest.Type}");
			}

			var message = new TaxcomDocflowSendEvent
			{
				EdoAccount = sender.TaxcomEdoSettings.EdoAccount,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo,
				EdoTaskId = documentTask.Id
			};
			await _publishEndpoint.Publish(message);
		}

		/// <summary>
		/// Обработка неформализованного документа заказа 
		/// </summary>
		/// <param name="orderDocumentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task SendInformalOrderDocument(
			int orderDocumentId,
			OrderDocumentFileData orderDocumentFileData, 
			CancellationToken cancellationToken
		)
		{
			var document = await _uow.Session.GetAsync<OutgoingInformalEdoDocument>(orderDocumentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning($"Документ {orderDocumentId} не найден");
				return;
			}

			var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status);
			if(isNotValidStatus)
			{
				_logger.LogError($"Документ {orderDocumentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			if(orderDocumentFileData.Image is null || orderDocumentFileData.Image.Length == 0)
			{
				_logger.LogWarning($"Файл документа {orderDocumentId} пустой.");
				return;
			}

			var informalEdoRequest = await _uow.Session.Query<InformalEdoRequest>()
				.Where(x => x.Task.Id == document.InformalDocumentTaskId)
				.FirstOrDefaultAsync(cancellationToken: cancellationToken);

			if(informalEdoRequest is null)
			{
				_logger.LogWarning($"Заявка на неформальный ЭДО для задачи с идентификатором {document.InformalDocumentTaskId} не найдена.");
				return;
			}

			var edoTask = informalEdoRequest.Task;
			if(edoTask is null)
			{
				_logger.LogWarning($"Задача ЭДО акта №{document.InformalDocumentTaskId} не найдена");
				return;
			}

			if(informalEdoRequest.IsBill)
			{
				var snapshot = JsonSerializer.Deserialize<InfoForCreatingEdoInformalOrderDocument>(
					informalEdoRequest.DocumentSnapshot);
				if(snapshot?.FileData != null && snapshot.FileData.Image == null)
				{
					snapshot.FileData.Image = orderDocumentFileData.Image;
				}
				if(snapshot?.IsBill != true || snapshot.FileData?.Image == null || snapshot.FileData.Image.Length == 0)
				{
					throw new InvalidOperationException($"У заявки счета №{informalEdoRequest.Id} отсутствует PDF снимка");
				}

				await _publishEndpoint.Publish(new TaxcomDocflowInformalDocumentSendEvent
				{
					EdoAccount = snapshot.OrganizationInfoForEdo.TaxcomEdoAccountId,
					EdoOutgoingDocumentId = document.Id,
					DocumentInfo = snapshot,
					EdoTaskId = edoTask.Id
				}, cancellationToken);
				return;
			}

			var order = await _uow.Session.GetAsync<OrderEntity>(orderDocumentFileData.OrderId, cancellationToken);
			if(order is null)
			{
				_logger.LogWarning($"Заказ №{orderDocumentFileData.OrderId} не найден");
				return;
			}

			var sender = order.Contract.Organization;
			if(sender.TaxcomEdoSettings is null)
			{
				_logger.LogWarning($"Настройки ЭДО Такском не найдены для организации отправителя {sender.Id}");
				return;
			}

			try
			{
				var informalDocument = order.OrderDocuments
					.FirstOrDefault(x => x.Type == informalEdoRequest.OrderDocumentType);

				if(informalDocument is null)
				{
					_logger.LogWarning($"Документ заказа {informalEdoRequest.OrderDocumentType} для заказа {order.Id} не найден");
					return;
				}

				var handler = _documentHandlerFactory.GetHandler(informalDocument.Type);
				var documentInfo = await handler.ProcessDocument(order, orderDocumentFileData, informalDocument.Id, cancellationToken);

				var message = new TaxcomDocflowInformalDocumentSendEvent
				{
					EdoAccount = sender.TaxcomEdoSettings.EdoAccount,
					EdoOutgoingDocumentId = document.Id,
					DocumentInfo = documentInfo,
					EdoTaskId = informalEdoRequest.Task.Id
				};

				await _publishEndpoint.Publish(message, cancellationToken);

				_logger.LogInformation($"Отправка неформализованного документа заказа №{orderDocumentId}");
			}
			catch(NotSupportedException ex)
			{
				_logger.LogWarning(ex, $"Неподдерживаемый тип документа: {informalEdoRequest.OrderDocumentType}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при отправке неформализованного документа заказа №{orderDocumentId}");
				throw;
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
