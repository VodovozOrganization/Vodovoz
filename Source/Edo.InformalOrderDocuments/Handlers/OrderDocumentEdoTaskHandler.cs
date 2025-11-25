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

namespace Edo.InformalOrderDocuments.Handlers
{
	public class OrderDocumentEdoTaskHandler
	{
		private readonly IUnitOfWork _uow;
		private readonly IInformalOrderDocumentHandlerFactory _handlerFactory;
		private readonly IBus _messageBus;
		private readonly ILogger<OrderDocumentEdoTaskHandler> _logger;

		public OrderDocumentEdoTaskHandler(
			IUnitOfWork uow,
			IInformalOrderDocumentHandlerFactory handlerFactory,
			IBus messageBus,
			ILogger<OrderDocumentEdoTaskHandler> logger
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task HandleNew(int orderDocumentEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<OrderDocumentEdoTask>(orderDocumentEdoTaskId, cancellationToken) 
				?? throw new InvalidOperationException($"Задача с идентификатором {orderDocumentEdoTaskId} не найдена.");

			var order = await _uow.Session.GetAsync<OrderEntity>(edoTask.Order.Id, cancellationToken) 
				?? throw new InvalidOperationException($"Заказ с идентификатором {edoTask.Order.Id} не найден.");

			try
			{
				var handler = _handlerFactory.GetHandler(edoTask.DocumentType);

				var orderDocument = order.OrderDocuments
					.Where(doc => doc.Type == edoTask.DocumentType).FirstOrDefault()
					?? throw new InvalidOperationException($"Документ типа {edoTask.DocumentType} не найден в заказе с идентификатором {order.Id}.");

				var result = await handler.ProcessDocumentAsync(order, orderDocument.Id, cancellationToken);

				var edoDocument = await CreateInformalDocument(edoTask.Id, cancellationToken);

				var fileDataMessage = new InformalDocumentFileDataSendEvent
				{
					DocumentId = edoDocument.Id,
					FileData = result
				};

				await _messageBus.Publish(fileDataMessage, cancellationToken);

				_logger.LogInformation($"Отправка PDF {orderDocument.Name} заказа №{order.Id}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при обработке задачи EDO документа заказа №{order.Id}, тип документа {edoTask.DocumentType}.");
				throw;
			}
			
		}

		private async Task<OutgoingInformalEdoDocument> CreateInformalDocument(int edoTaskId, CancellationToken cancellationToken)
		{
			var customerEdoDocument = new OutgoingInformalEdoDocument
			{
				DocumentTaskId = edoTaskId,
				Status = EdoDocumentStatus.NotStarted,
				EdoType = EdoType.Taxcom,
				DocumentType = EdoDocumentType.InformalOrderDocument,
				Type = OutgoingEdoDocumentType.InformalOrderDocument
			};

			await _uow.SaveAsync(customerEdoDocument, cancellationToken: cancellationToken);
			return customerEdoDocument;
		}
	}
}
