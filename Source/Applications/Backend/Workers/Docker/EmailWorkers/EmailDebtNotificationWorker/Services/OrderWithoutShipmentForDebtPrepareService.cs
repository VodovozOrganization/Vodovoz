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
using VodovozBusiness.EntityRepositories.Nodes;

namespace EmailDebtNotificationWorker.Services
{
	public class OrderWithoutShipmentForDebtPrepareService : IOrderWithoutShipmentForDebtPrepareService
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGenericRepository<OrderWithoutShipmentForDebt> _orderWithoutShipmentForDebtRepository;
		private const int _daysToCheckExistingBills = 3;

		public OrderWithoutShipmentForDebtPrepareService(
			IEmployeeRepository employeeRepository,
			IGenericRepository<OrderWithoutShipmentForDebt> orderWithoutShipmentForDebtRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderWithoutShipmentForDebtRepository = orderWithoutShipmentForDebtRepository ?? throw new ArgumentNullException(nameof(orderWithoutShipmentForDebtRepository));
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

			var existingOrdersForDebt = (await _orderWithoutShipmentForDebtRepository.GetAsync(
				unitOfWork,
				x => x.CreateDate.Value >= dateFrom
				  && counterpartyIds.Contains(x.Client.Id),
				cancellationToken: cancellationToken))
			.Value;

			var notificationInfos = new List<OrderWithoutShipmentForDebtNotificationInfo>(overdueDebtAggregateNodes.Count);

			var author = _employeeRepository.GetEmployeeForCurrentUser(unitOfWork);

			foreach(var node in overdueDebtAggregateNodes)
			{
				var notificationInfo = new OrderWithoutShipmentForDebtNotificationInfo
				{
					OverdueDebtDays = node.OverdueDebtDays,
					OrderWithoutShipmentForDebt = existingOrdersForDebt.FirstOrDefault(x =>
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
