using Edo.Transfer.Routine.Options;
using Edo.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Delivery;

namespace Edo.Transfer.Routine.Services
{
	public class ClosingDocumentsOrdersUpdSendService
	{
		private readonly ILogger<ClosingDocumentsOrdersUpdSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> _optionsMonitor;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;

		private readonly IEnumerable<OrderStatus> _orderStatusesToSendUpd =
			new [] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

		public ClosingDocumentsOrdersUpdSendService(
			ILogger<ClosingDocumentsOrdersUpdSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> optionsMonitor,
			IOrderRepository orderRepository,
			IEdoRequestCreatedEventPublisher edoRequestCreatedEventPublisher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_optionsMonitor = optionsMonitor;
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_edoRequestCreatedEventPublisher = edoRequestCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(edoRequestCreatedEventPublisher));
		}

		public async Task Send(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ClosingDocumentsOrdersUpdSendService)))
			{
				var unprocessedEdoRequests = await GetUnprocessedClosingDocumentUpdEdoRequests(uow, cancellationToken);
				var orders = await GetCloseDocumentOrdersToSendEdoRequest(uow, cancellationToken);
				var edoRequests = await CreateEdoRequests(uow, orders, cancellationToken);

				await PublishEdoRequestCreatedEvents(unprocessedEdoRequests.Concat(edoRequests));
			}
		}

		private async Task<IEnumerable<OrderEntity>> GetCloseDocumentOrdersToSendEdoRequest(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы для отправки ЭДО УПД");

			var orders = await _orderRepository.GetOrdersForClosingDocumentUpdEdoAsync(
				uow,
				_optionsMonitor.CurrentValue.MaxDaysFromDeliveryDate,
				_deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId,
				_orderStatusesToSendUpd,
				cancellationToken);

			_logger.LogInformation("Найдено {OrdersCount} заказов для отправки ЭДО УПД", orders.Count());

			return orders;
		}

		private async Task<IEnumerable<PrimaryEdoRequest>> GetUnprocessedClosingDocumentUpdEdoRequests(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем необработанные заявки на отправку ЭДО УПД по заказам Закр.Док");

			var edoRequests = await _orderRepository.GetUnprocessedClosingDocumentUpdEdoRequestsAsync(
				uow,
				_optionsMonitor.CurrentValue.MaxDaysFromDeliveryDate,
				_deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId,
				_orderStatusesToSendUpd,
				cancellationToken);

			var filteredRequests = FilterRequestsWithoutAccountableInTrueMarkItems(edoRequests).ToList();

			_logger.LogInformation("Найдено {RequestsCount} необработанных заявок на отправку ЭДО УПД", filteredRequests.Count);

			return filteredRequests;
		}

		private async Task<IEnumerable<PrimaryEdoRequest>> CreateEdoRequests(IUnitOfWork uow, IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Создаем заявки на ЭДО УПД");

			var edoRequests = new List<PrimaryEdoRequest>();

			foreach(var order in orders)
			{
				if(order is null)
				{
					continue;
				}

				if(order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark))
				{
					_logger.LogError(
						"В заказе \"Закр док\" имеются маркированные товары. Отправка документов по ЭДО недоступна. OrderId: {OrderId}",
						order.Id);

					continue;
				}

				var edoRequest = CreateEdoRequests(order);

				await uow.SaveAsync(edoRequest, cancellationToken: cancellationToken);

				edoRequests.Add(edoRequest);
			}

			if(edoRequests.Any())
			{
				await uow.CommitAsync(cancellationToken);
			}

			_logger.LogInformation("Создано {RequestsCount} заявок", edoRequests.Count);

			return edoRequests;
		}

		private IEnumerable<PrimaryEdoRequest> FilterRequestsWithoutAccountableInTrueMarkItems(IEnumerable<PrimaryEdoRequest> edoRequests)
		{
			foreach(var edoRequest in edoRequests)
			{
				if(edoRequest?.Order is null)
				{
					continue;
				}

				if(edoRequest.Order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark))
				{
					_logger.LogError(
						"В заказе \"Закр док\" имеются маркированные товары. Отправка документов по ЭДО недоступна. RequestId: {RequestId}. OrderId: {OrderId}",
						edoRequest.Id,
						edoRequest.Order.Id);

					continue;
				}

				yield return edoRequest;
			}
		}

		private PrimaryEdoRequest CreateEdoRequests(OrderEntity order)
		{
			var edoRequest = new PrimaryEdoRequest
			{
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
			};

			return edoRequest;
		}

		private async Task PublishEdoRequestCreatedEvents(IEnumerable<PrimaryEdoRequest> edoRequests)
		{
			_logger.LogInformation("Отправляем события о создании новых заявок по ЭДО");

			if(edoRequests is null || !edoRequests.Any())
			{
				_logger.LogInformation("Нет новых заявок по ЭДО для отправки событий");

				return;
			}

			foreach(var edoRequest in edoRequests)
			{
				try
				{
					_logger.LogInformation("Отправляем событие о создании новой заявки по ЭДО. RequestId: {RequestId}.", edoRequest.Id);

					await _edoRequestCreatedEventPublisher.Publish(
						edoRequest.Id,
						"Формирование УПД закрывающих документов");

					_logger.LogInformation("Событие о создании новой заявки по ЭДО отправлено успешно");
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Ошибка при отправке события на создание новой заявки по ЭДО. RequestId: {RequestId}.",
						edoRequest.Id);

					continue;
				}
			}
		}
	}
}
