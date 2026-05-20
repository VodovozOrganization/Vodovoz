using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VodovozBusiness.EntityRepositories.Nodes;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Сервис для закрытия поставок у контрагентов с просроченной задолженностью и открытия поставок у контрагентов с отсутствующей просроченной задолженностью
	/// </summary>
	public interface IClosingDeliveriesService
	{
		/// <summary>
		/// Закрытие поставок у контрагентов с просроченной задолженностью
		/// </summary>
		Task<IReadOnlyCollection<OverdueDebtOverPeriodLimitAggregateNode>> CloseDeliveriesForDebtorsAsync(IUnitOfWork unitOfWork, int? counterpartyId = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Открытие поставок у контрагента с отсутствующей просроченной задолженностью
		/// </summary>
		Task CheckAndOpenDeliveriesAsync(IUnitOfWork unitOfWork, int counterpartyId, CancellationToken cancellationToken = default);
	}
}
