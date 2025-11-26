using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Factories;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Edo.InformalOrderDocuments.Handlers
{
	/// <summary>
	/// Обработчик задачи ЭДО документа заказа
	/// </summary>
	public class OrderDocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly IInformalOrderDocumentHandlerFactory _handlerFactory;
		private readonly IBus _messageBus;
		private readonly ILogger<OrderDocumentEdoTaskHandler> _logger;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;

		public OrderDocumentEdoTaskHandler(
			IUnitOfWork uow,
			IInformalOrderDocumentHandlerFactory handlerFactory,
			IBus messageBus,
			ILogger<OrderDocumentEdoTaskHandler> logger,
			EdoProblemRegistrar edoProblemRegistrar
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
		}

		/// <summary>
		/// Обработка новой задачи ЭДО документа заказа
		/// </summary>
		/// <param name="orderDocumentEdoTaskId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleNew(int orderDocumentEdoTaskId, CancellationToken cancellationToken)
		{
			var informalEdoRequest = await _uow.Session.Query<InformalEdoRequest>()
				.Where(x => x.Task.Id == orderDocumentEdoTaskId)
				.FirstOrDefaultAsync(cancellationToken: cancellationToken);

			if(informalEdoRequest == null)
			{
				_logger.LogWarning($"Заявка на неформальный ЭДО для задачи с идентификатором {orderDocumentEdoTaskId} не найдена.");
				return;
			}

			var edoTask = informalEdoRequest.Task;
			if(edoTask == null)
			{
				_logger.LogWarning($"Задача с идентификатором ЭДО №{informalEdoRequest.Task} не найдена");
				return;
			}

			var order = await _uow.Session.GetAsync<OrderEntity>(informalEdoRequest.Order.Id, cancellationToken);
			if(order == null)
			{
				_logger.LogWarning($"Заказ с идентификатором {informalEdoRequest.Order.Id} не найден.");
				return;
			}

			try
			{
				var handler = _handlerFactory.GetHandler(informalEdoRequest.OrderDocumentType);

				var orderDocument = order.OrderDocuments
					.Where(doc => doc.Type == informalEdoRequest.OrderDocumentType).FirstOrDefault();
				if(orderDocument == null)
				{
					_logger.LogWarning($"Документ типа {informalEdoRequest.OrderDocumentType} для заказа №{order.Id} не найден.");
				}

				var result = await handler.ProcessDocumentAsync(order, orderDocument.Id, cancellationToken);

				var edoDocument = await CreateInformalDocument(edoTask.Id, cancellationToken);

				var fileDataMessage = new InformalDocumentFileDataSendEvent
				{
					DocumentId = edoDocument.Id,
					FileData = result
				};

				edoTask.Status = EdoTaskStatus.InProgress;

				await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);

				await _messageBus.Publish(fileDataMessage, cancellationToken);

				_logger.LogInformation($"Отправка PDF {orderDocument.Name} заказа №{order.Id}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при обработке задачи EDO документа заказа №{order.Id}, тип документа {informalEdoRequest.OrderDocumentType}.");
				throw;
			}
			
		}

		private async Task<OutgoingInformalEdoDocument> CreateInformalDocument(int edoTaskId, CancellationToken cancellationToken)
		{
			var edoDocument = new OutgoingInformalEdoDocument
			{
				InformalDocumentTaskId = edoTaskId,
				Status = EdoDocumentStatus.NotStarted,
				EdoType = EdoType.Taxcom,
				DocumentType = EdoDocumentType.InformalOrderDocument,
				Type = OutgoingEdoDocumentType.InformalOrderDocument
			};

			await _uow.SaveAsync(edoDocument, cancellationToken: cancellationToken);
			return edoDocument;
		}

		/// <summary>
		/// Обработка принятого документа заказа
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleAccepted(int documentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OutgoingInformalEdoDocument>(documentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning("Документ №{DocumentId} не найден.", documentId);
				return;
			}

			var edoTask = await _uow.Session.GetAsync<OrderDocumentEdoTask>(document.InformalDocumentTaskId, cancellationToken);
			if(edoTask == null)
			{
				_logger.LogWarning("Задача ЭДО №{DocumentEdoTaskId} не найдена.", document.InformalDocumentTaskId);
				return;
			}

			edoTask.Status = EdoTaskStatus.Completed;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		/// <summary>
		/// Обработка проблемы с документом заказа
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleProblem(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<OutgoingInformalEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{DocumentId} не найден документ.", documentId);
			}

			var documentTask = await _uow.Session.GetAsync<OrderDocumentEdoTask>(document.InformalDocumentTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				documentTask, cancellationToken, "Возникла проблема с документооборотом, не завершился на стороне ЭДО провайдера");
		}

		/// <summary>
		/// Обработка аннулированого документа заказа
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleCancelled(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<OutgoingInformalEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{DocumentId} не найден документ.", documentId);
			}

			var documentTask = await _uow.Session.GetAsync<OrderDocumentEdoTask>(document.InformalDocumentTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				documentTask, cancellationToken, "Документооборот был отменен");
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
