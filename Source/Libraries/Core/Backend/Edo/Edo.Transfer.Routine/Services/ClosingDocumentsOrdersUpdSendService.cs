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
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Delivery;

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

			await Task.CompletedTask;
		}

		private async Task<IEnumerable<OrderEntity>> GetCloseDocumentOrdersToSendEdoRequest(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var orders =
					from order in uow.Session.Query<OrderEntity>()
					join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
					join er in uow.Session.Query<OrderEdoRequest>() on order.Id equals er.Order.Id into edoRequests
					from edoRequest in edoRequests.DefaultIfEmpty()
					where
					order.PaymentType == PaymentType.Cashless
					&& order.Client.IsNewEdoProcessing
					select order;

				return await orders.ToListAsync(cancellationToken);
			}
		}
	}
}
