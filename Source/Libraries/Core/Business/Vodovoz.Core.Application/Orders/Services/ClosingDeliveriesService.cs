using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.EntityRepositories.Nodes;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class ClosingDeliveriesService : IClosingDeliveriesService
	{
		private readonly ILogger<ClosingDeliveriesService> _logger;
		private readonly IClosingDeliveriesSettings _closingDeliveriesSettings;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly int[] _organizationsIds;

		private const decimal _debtThreshold = 0m;

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
			IClosingDeliveriesSettings closingDeliveriesSettings,
			IOrganizationSettings organizationSettings,
			IGenericRepository<CounterpartyEntity> counterpartyRepository,
			IOrderRepository orderRepository,
			IEmployeeRepository employeeRepository,
			ICounterpartySettings counterpartySettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_closingDeliveriesSettings = closingDeliveriesSettings ?? throw new ArgumentNullException(nameof(closingDeliveriesSettings));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationsIds = new[] { organizationSettings.VodovozOrganizationId, organizationSettings.KulerServiceOrganizationId };
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
		}

		public async Task<IReadOnlyCollection<OverdueDebtOverPeriodLimitAggregateNode>> CloseDeliveriesForDebtorsAsync(IUnitOfWork unitOfWork, int? counterpartyId = null, CancellationToken cancellationToken = default)
		{
			var overdueDebtOverPeriodLimitRows = await
				_orderRepository.GetWithoutClosedDeliveriesCounterpartiesOverdueDebts(
					unitOfWork,
					_closingDeliveriesSettings.DaysBeforeClosingDeliveries,
					_organizationsIds,
					_orderStatuses,
					_counterpartyTypes,
					_counterpartySettings.CounterpartyFromTenderId,
					_debtThreshold,
					counterpartyId,
					cancellationToken);

			if(!overdueDebtOverPeriodLimitRows.Any())
			{
				_logger.LogInformation("Нет контрагентов с просроченной дебиторской задолженностью по заказам");

				return overdueDebtOverPeriodLimitRows;
			}

			// Закрытие поставок, только если есть долг по организации ВВ

			var vodovozOrganizationRows = overdueDebtOverPeriodLimitRows.Where(x => x.Organization.Id == _organizationSettings.VodovozOrganizationId).ToList();

			if(!vodovozOrganizationRows.Any())
			{
				return vodovozOrganizationRows;
			}

			var employee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			foreach(var vodovozOrganizationRow in vodovozOrganizationRows)
			{
				var counterparty = vodovozOrganizationRow.Counterparty;

				counterparty.CloseDeliveryDebtType = DebtType.BlockedByRobot;
				
				var closingComment = $"Робот; {DateTime.Now:dd.MM.yy HH:mm}";

				var counterpartyOverdueRows = overdueDebtOverPeriodLimitRows.Where(x => x.Counterparty.Id == counterparty.Id);

				foreach(var counterpartyOverdueRow in counterpartyOverdueRows)
				{
					closingComment += @$"; Долг по {counterpartyOverdueRow.Organization.Name}: {counterpartyOverdueRow.DebtSum}";
				}				

				counterparty.CloseDeliveryComment = closingComment;

				counterparty.ToggleDeliveryOption(employee, true);

				await unitOfWork.SaveAsync(counterparty, cancellationToken: cancellationToken);
			}

			_logger.LogInformation("Закрыты поставки {CounterpartiesCount} контрагентам", vodovozOrganizationRows.Count());

			return overdueDebtOverPeriodLimitRows;
		}

		public async Task CheckAndOpenDeliveriesAsync(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			CancellationToken cancellationToken = default)
		{
			var hasOverdueDebt = await _orderRepository.HasClosedDeliveriesCounterpartyWithOverdueDebtsAsync(
				unitOfWork,
				_closingDeliveriesSettings.DaysBeforeClosingDeliveries,
				_organizationsIds,
				_orderStatuses,
				counterpartyId,
				cancellationToken);

			if(hasOverdueDebt)
			{
				return;
			}

			var counterparty = unitOfWork.GetById<Counterparty>(counterpartyId);

			if(counterparty == null)
			{
				return;
			}

			if(!counterparty.IsDeliveriesClosed)
			{
				return;
			}

			if(counterparty.RevenueStatus != RevenueStatus.Active)
			{
				return;
			}

			var employee = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			counterparty.ToggleDeliveryOption(employee, true);

			await unitOfWork.SaveAsync(counterparty, cancellationToken: cancellationToken);

			_logger.LogInformation("Открыты поставки контрагенту {CounterpartyId}", counterpartyId);
		}
	}
}
