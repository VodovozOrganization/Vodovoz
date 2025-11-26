using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Factories;
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
	}
}
