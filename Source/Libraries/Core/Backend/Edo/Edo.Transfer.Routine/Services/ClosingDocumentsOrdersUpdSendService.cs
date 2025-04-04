using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using Vodovoz.Settings.Delivery;
using Type = Vodovoz.Core.Domain.Documents.Type;

namespace Edo.Transfer.Routine.Services
{
	public class ClosingDocumentsOrdersUpdSendService
	{
		private readonly ILogger<ClosingDocumentsOrdersUpdSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;

		public ClosingDocumentsOrdersUpdSendService(
			ILogger<ClosingDocumentsOrdersUpdSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IServiceScopeFactory serviceScopeFactory,
			IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
		}

		public async Task Send(CancellationToken cancellationToken)
		{
			var orders = await GetCloseDocumentOrdersToSendEdoRequest(cancellationToken);

			foreach(var order in orders)
			{

			}

			await Task.CompletedTask;
		}

		private async Task<IEnumerable<OrderEntity>> GetCloseDocumentOrdersToSendEdoRequest(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Получение заказов с \"Закр док\" для отправки УПД"))
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
					//Проверка, что Закр Док
					order.PaymentType == PaymentType.Cashless
					&& client.IsNewEdoProcessing
					&& client.ConsentForEdoStatus == ConsentForEdoStatus.Agree
					&& edoContainer.Id == null
					&& edoRequest.Id == null
					select order;

				return await orders.ToListAsync(cancellationToken);
			}
		}

		private async Task CreateEdoRequests(IEnumerable<OrderEntity> order)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Создание запросов на отправку УПД для заказов \"Закр док\""))
			{
				//var ordersHavingRequests =


				//OrderEdoRequest edoRequest = _uow.Session.Query<OrderEdoRequest>()
				//.Where(x => x.Order.Id == vodovozOrder.Id)
				//.Take(1)
				//.SingleOrDefault();

				//var edoRequestCreated = false;
				//if(!vodovozOrder.IsNeedIndividualSetOnLoad && edoRequest == null)
				//{
				//	edoRequest = CreateEdoRequests(vodovozOrder, routeListAddress);
				//	edoRequestCreated = true;
				//}

				//_uow.Commit();

				//if(edoRequestCreated)
				//{
				//	await _edoMessageService.PublishEdoRequestCreatedEvent(edoRequest.Id);
				//}
			}
		}
	}
}
