using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VodovozBusiness.EntityRepositories.Nodes;

namespace EmailDebtNotificationWorker.Services.ClosingDeliveries
{
	/// <summary>
	/// Сервис для подготовки информации о счетах без отгрузки на долг для уведомления о просроченной задолженности
	/// </summary>
	public interface IOrderWithoutShipmentForDebtPreparer
	{
		/// <summary>
		/// Подготовить информацию о счетах без отгрузки на долг для уведомления о просроченной задолженности
		/// </summary>
		Task<IReadOnlyList<OrderWithoutShipmentForDebtNotificationInfo>> PrepareInfo(
			IUnitOfWork unitOfWork,
			IReadOnlyCollection<OverdueDebtOverPeriodLimitAggregateNode> nodes,
			CancellationToken cancellationToken);
	}
}
