using Edo.Transfer.Routine.Options;
using Edo.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Delivery;
using Type = Vodovoz.Core.Domain.Documents.Type;

namespace Edo.Transfer.Routine.Services
{
	public class ClosingDocumentsOrdersUpdSendService
	{
		private readonly ILogger<ClosingDocumentsOrdersUpdSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> _optionsMonitor;
		private readonly MessageService _edoMessageService;

		private readonly IEnumerable<OrderStatus> _orderStatusesToSendUpd =
			new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

		public ClosingDocumentsOrdersUpdSendService(
			ILogger<ClosingDocumentsOrdersUpdSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> optionsMonitor,
			MessageService edoMessageService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_optionsMonitor = optionsMonitor;
			_edoMessageService = edoMessageService ?? throw new ArgumentNullException(nameof(edoMessageService));
		}

		public async Task Send(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ClosingDocumentsOrdersUpdSendService)))
			{
				var orders = await GetCloseDocumentOrdersToSendEdoRequest(uow, cancellationToken);
				var edoRequests = await CreateEdoRequests(uow, orders, cancellationToken);

				await PublishEdoRequestCreatedEvents(edoRequests);
			}
		}

		private async Task<IEnumerable<OrderEntity>> GetCloseDocumentOrdersToSendEdoRequest(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var orders =
				from order in uow.Session.Query<OrderEntity>()
				join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
				join er in uow.Session.Query<OrderEdoRequest>() on order.Id equals er.Order.Id into edoRequests
				from edoRequest in edoRequests.DefaultIfEmpty()
				join ec in uow.Session.Query<EdoContainerEntity>()
					on new { OrderId = order.Id, DocType = Type.Upd } equals new { OrderId = ec.Order.Id, DocType = ec.Type } into edoContainers
				from edoContainer in edoContainers.DefaultIfEmpty()
				where
				order.PaymentType == PaymentType.Cashless
				&& order.DeliveryDate >= DateTime.Today.AddDays(-_optionsMonitor.CurrentValue.MaxDaysFromDeliveryDate)
				&& order.DeliverySchedule.Id == _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId
				&& _orderStatusesToSendUpd.Contains(order.OrderStatus)
				&& client.IsNewEdoProcessing
				&& client.ConsentForEdoStatus == ConsentForEdoStatus.Agree
				&& edoContainer.Id == null
				&& edoRequest.Id == null
				select order;

			return await orders.ToListAsync(cancellationToken);
		}

		private async Task<IEnumerable<OrderEdoRequest>> CreateEdoRequests(IUnitOfWork uow, IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
		{
			var edoRequests = new List<OrderEdoRequest>();

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

				edoRequests.Add(await CreateEdoRequests(uow, order, cancellationToken));
			}

			await uow.CommitAsync(cancellationToken);

			return edoRequests;
		}

		private async Task<OrderEdoRequest> CreateEdoRequests(IUnitOfWork uow, OrderEntity order, CancellationToken cancellationToken)
		{
			var edoRequest = new OrderEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
			};

			await uow.SaveAsync(edoRequest, cancellationToken: cancellationToken);

			return edoRequest;
		}

		private async Task PublishEdoRequestCreatedEvents(IEnumerable<OrderEdoRequest> edoRequests)
		{
			foreach(var edoRequest in edoRequests)
			{
				try
				{
					await _edoMessageService.PublishEdoRequestCreatedEvent(edoRequest.Id);
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Ошибка при отправке события на создание новой заявки по ЭДО. Id запроса: {RequestId}.",
						edoRequest.Id);

					continue;
				}
			}
		}
	}
}
