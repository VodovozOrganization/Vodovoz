using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.EntityRepositories.Nodes;

namespace EmailDebtNotificationWorker.Services.ClosingDeliveries
{
	public class OrderWithoutShipmentForDebtPreparer : IOrderWithoutShipmentForDebtPreparer
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGenericRepository<OrderWithoutShipmentForDebt> _orderWithoutShipmentForDebtRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private const int _daysToCheckExistingBills = 3;

		public OrderWithoutShipmentForDebtPreparer(
			IEmployeeRepository employeeRepository,
			IGenericRepository<OrderWithoutShipmentForDebt> orderWithoutShipmentForDebtRepository,
			IOrganizationSettings organizationSettings)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderWithoutShipmentForDebtRepository = orderWithoutShipmentForDebtRepository ?? throw new ArgumentNullException(nameof(orderWithoutShipmentForDebtRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}

		public async Task<IReadOnlyList<OrderWithoutShipmentForDebtNotificationInfo>> PrepareInfo(
			IUnitOfWork unitOfWork,
			IReadOnlyCollection<OverdueDebtOverPeriodLimitAggregateNode> overdueDebtAggregateNodes,
			CancellationToken cancellationToken)
		{
			if(!overdueDebtAggregateNodes.Any())
			{
				return Array.Empty<OrderWithoutShipmentForDebtNotificationInfo>();
			}

			var counterpartyIds = overdueDebtAggregateNodes.Select(x => x.Counterparty.Id).ToHashSet();

			var dateFrom = DateTime.Now.AddDays(-_daysToCheckExistingBills);

			var existingOrdersWithoutShipmentForDebt = (await _orderWithoutShipmentForDebtRepository.GetAsync(
				unitOfWork,
				x => x.CreateDate.Value >= dateFrom
				  && counterpartyIds.Contains(x.Client.Id),
				cancellationToken: cancellationToken))
			.Value;

			var notificationInfos = new List<OrderWithoutShipmentForDebtNotificationInfo>(overdueDebtAggregateNodes.Count);

			var author = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			foreach(var node in overdueDebtAggregateNodes)
			{
				// Если по ВВ нет долга, то и по остальным не отправляем
				if(node.Organization.Id != _organizationSettings.VodovozOrganizationId)
				{
					var vodovozNode = overdueDebtAggregateNodes.FirstOrDefault(
						x => x.Organization.Id == _organizationSettings.VodovozOrganizationId
						&& x.Counterparty.Id == node.Counterparty.Id);
					
					if(vodovozNode is null)
					{
						continue;
					}
				}

				var notificationInfo = new OrderWithoutShipmentForDebtNotificationInfo
				{
					OverdueDebtDays = node.OverdueDebtDays,
					OldestDebtOrderDate = node.OldestDebtOrderDate,
					OrderWithoutShipmentForDebt = existingOrdersWithoutShipmentForDebt.LastOrDefault(x =>
						x.Client.Id == node.Counterparty.Id
						&& x.Organization.Id == node.Organization.Id)
				};

				if(notificationInfo.OrderWithoutShipmentForDebt == null)
				{
					var newOrderForDebt = new OrderWithoutShipmentForDebt
					{
						Client = node.Counterparty,
						Organization = node.Organization,
						DebtSum = node.DebtSum,
						Author = author
					};

					await unitOfWork.SaveAsync(newOrderForDebt, cancellationToken: cancellationToken);

					notificationInfo.OrderWithoutShipmentForDebt = newOrderForDebt;
				}

				notificationInfos.Add(notificationInfo);
			}

			return notificationInfos;
		}
	}
}
