using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class ClosingDeliveriesService : IClosingDeliveriesService
	{
		private readonly ILogger<ClosingDeliveriesService> _logger;
		private readonly IClosingDeliveriesSettings _closingDeliveriesSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmployeeRepository _employeeRepository;

		private readonly int[] _organizationsIds;

		private readonly OrderStatus[] _orderStatuses =
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private readonly CounterpartyType[] _counterpartyTypes =
		{
			CounterpartyType.Buyer
		};

		public ClosingDeliveriesService(
			ILogger<ClosingDeliveriesService> logger,
			IClosingDeliveriesSettings blockingDeliveriesSettings,
			IOrganizationSettings organizationSettings,
			IGenericRepository<CounterpartyEntity> counterpartyRepository,
			IOrderRepository orderRepository,
			IEmployeeRepository employeeRepository)
		{
			if(organizationSettings is null)
			{
				throw new ArgumentNullException(nameof(organizationSettings));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_closingDeliveriesSettings = blockingDeliveriesSettings ?? throw new ArgumentNullException(nameof(blockingDeliveriesSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationsIds = new[] { organizationSettings.VodovozOrganizationId, organizationSettings.KulerServiceOrganizationId };
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public async Task CheckAndCloseDeliveriesAsync(IUnitOfWork unitOfWork, int? counterpartyId = null, CancellationToken cancellationToken = default)
		{
			var ordersWithDebtOverdue = await
				_orderRepository.GetOverdueDebtQuery(
					unitOfWork,
					_closingDeliveriesSettings.DaysBeforeClosingDeliveries,
					_organizationsIds,
					_orderStatuses,
					_counterpartyTypes,
					counterpartyId)
					.Where(x => !x.Counterparty.IsDeliveriesClosed)
					.ToListAsync(cancellationToken);

			var counterpartiesWithDebtOverdueNodes = ordersWithDebtOverdue
			   .GroupBy(x => x.Counterparty.Id)
			   .Select(g => new
			   {
				   Counterparty = g.First().Counterparty,
				   OrderIds = g.Select(x => x.OrderId).ToList(),
				   Debt = g.Sum(x => x.Debt)
			   });

			if(!counterpartiesWithDebtOverdueNodes.Any())
			{
				_logger.LogInformation("Нет контрагентов с просроченной дебиторской задолженностью по заказам.");

				return;
			}

			var employee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			foreach(var counterpartyNode in counterpartiesWithDebtOverdueNodes)
			{
				var counterparty = counterpartyNode.Counterparty;

				counterparty.CloseDeliveryDebtType = DebtType.ShortTerm;

				counterparty.CloseDeliveryComment =
					$"Поставки закрыты автоматически в связи с неоплаченной просроченной дебиторской задолженностью по заказам {string.Join(", ", counterpartyNode.OrderIds)}";

				counterparty.ToggleDeliveryOption(employee, true);

				await unitOfWork.SaveAsync(counterparty, cancellationToken: cancellationToken);
			}

			_logger.LogInformation("Закрыты поставки {CounterpartiesCount} контрагентам", counterpartiesWithDebtOverdueNodes.Count());
		}

		public async Task CheckAndOpenDeliveriesAsync(IUnitOfWork unitOfWork, int counterpartyId, CancellationToken cancellationToken = default)
		{
			var ordersWithDebtOverdue = await
				_orderRepository.GetOverdueDebtQuery(
						unitOfWork,
						_closingDeliveriesSettings.DaysBeforeClosingDeliveries,
						_organizationsIds,
						_orderStatuses,
						_counterpartyTypes,
						counterpartyId)
					.Where(x => x.Counterparty.IsDeliveriesClosed)
					.ToListAsync(cancellationToken);

			if(ordersWithDebtOverdue.Any())
			{
				return;
			}

			var employee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			var counterparty = unitOfWork.GetById<Counterparty>(counterpartyId);

			counterparty.ToggleDeliveryOption(employee, true);

			await unitOfWork.SaveAsync(counterparty, cancellationToken: cancellationToken);

			_logger.LogInformation("Открыты поставки контрагенту {CounterpartyId}", counterpartyId);
		}
	}
}
