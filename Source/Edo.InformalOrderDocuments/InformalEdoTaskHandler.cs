using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;

namespace Edo.InformalOrderDocuments
{
	/// <summary>
	/// Обработчик задач ЭДО по неформализованным документам
	/// </summary>
	public class InformalEdoTaskHandler : IDisposable
	{
		private readonly ILogger<InformalEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IBus _messageBus;
		private readonly IPrintableDocumentSaver _printableDocumentSaver;
		private readonly IEquipmentTransferFileDataFactory _equipmentTransferFileDataFactory;

		public InformalEdoTaskHandler(
			ILogger<InformalEdoTaskHandler> logger,
			IUnitOfWork uow,
			IBus messageBus,
			IPrintableDocumentSaver printableDocumentSaver,
			IEquipmentTransferFileDataFactory equipmentTransferFileDataFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_equipmentTransferFileDataFactory = equipmentTransferFileDataFactory ?? throw new ArgumentNullException(nameof(equipmentTransferFileDataFactory));
		}

		/// <summary>
		/// Отправка неформализованного документа через ЭДО
		/// </summary>
		/// <param name="OrderDocumentEdoTaskId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task SendInformalDocument(int OrderDocumentEdoTaskId, CancellationToken cancellationToken)
		{
			var customerDocument = await CreateInformalDocument(OrderDocumentEdoTaskId, cancellationToken);
			var edoTask = await _uow.Session.GetAsync<EquipmentTransferEdoTask>(OrderDocumentEdoTaskId, cancellationToken);
			var message = new EquipmentTransferDocumentSendEvent { EquipmentTransferDocumentId = customerDocument.Id };

			edoTask.Status = EdoTaskStatus.InProgress;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
		}

		/// <summary>
		/// Обработка документа акта приёма-передачи оборудования
		/// </summary>
		/// <param name="equipmentTransferDocumentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleEquipmentTransferDocument(int equipmentTransferDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<EquipmentTransferEdoDocument>(equipmentTransferDocumentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning($"Документ {equipmentTransferDocumentId} не найден");
				return;
			}

			if(document.Status.IsIn(
				EdoDocumentStatus.InProgress,
				EdoDocumentStatus.Succeed
				))
			{
				_logger.LogError($"Документ {equipmentTransferDocumentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var edoTask = await _uow.Session.GetAsync<EquipmentTransferEdoTask>(document.DocumentTaskId, cancellationToken);
			if(edoTask == null)
			{
				_logger.LogWarning($"Задача ЭДО акта №{document.DocumentTaskId} не найдена");
				return;
			}

			var order = await _uow.Session.GetAsync<OrderEntity>(edoTask.OrderEdoRequest.Order.Id, cancellationToken);
			if(order == null)
			{
				_logger.LogWarning($"Заказ для акта приёма-передачи №{edoTask.Id} не найден");
				return;
			}

			var sender = order.Contract.Organization;
			if(sender.TaxcomEdoSettings == null)
			{
				_logger.LogWarning($"Настройки ЭДО Такском не найдены для организации отправителя {sender.Id}");
				return;
			}

			try
			{
				if(!(order.OrderDocuments
					.FirstOrDefault(x => x.Type == OrderDocumentType.EquipmentTransfer) is EquipmentTransferDocumentEntity equipmentTransferDocument))
				{
					_logger.LogWarning($"Акт приёма-передачи оборудования для заказа {order.Id} не найден");
					return;
				}

				var pdfBytes = _printableDocumentSaver.SaveToPdf(equipmentTransferDocument);

				var documentDate = equipmentTransferDocument.DocumentDate
					?? order.DeliveryDate
					?? order.CreateDate
					?? DateTime.Now;

				var fileData = _equipmentTransferFileDataFactory.CreateEquipmentTransferFileData(
					order.Id,
					documentDate,
					pdfBytes);

				var fileDataMessage = new InformalDocumentFileDataSendEvent
				{
					DocumentId = document.Id,
					FileData = fileData
				};

				await _messageBus.Publish(fileDataMessage, cancellationToken);

				_logger.LogInformation($"Отправка PDF документа заказа №{equipmentTransferDocumentId}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при отправкеPDF документа заказа №{equipmentTransferDocumentId}");
				throw;
			}
		}

		private async Task<EquipmentTransferEdoDocument> CreateInformalDocument(int edoTaskId, CancellationToken cancellationToken)
		{
			var customerEdoDocument = new EquipmentTransferEdoDocument
			{
				DocumentTaskId = edoTaskId,
				Status = EdoDocumentStatus.NotStarted,
				EdoType = EdoType.Taxcom,
				DocumentType = EdoDocumentType.EquipmentTransfer,
				Type = OutgoingEdoDocumentType.EquipmentTransfer
			};

			await _uow.SaveAsync(customerEdoDocument, cancellationToken: cancellationToken);
			return customerEdoDocument;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
