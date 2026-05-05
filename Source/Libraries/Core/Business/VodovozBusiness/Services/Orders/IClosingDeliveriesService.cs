using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace VodovozBusiness.Services.Orders
{
	public interface IClosingDeliveriesService
	{
		/// <summary>
		/// Закрытие поставок у контрагентов с просроченной задолженностью
		/// </summary>
		Task CheckAndCloseDeliveriesAsync(IUnitOfWork unitOfWork, int? counterpartyId = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Открытие поставок у контрагента с отсутствующей просроченной задолженностью
		/// </summary>
		Task CheckAndOpenDeliveriesAsync(IUnitOfWork unitOfWork, int counterpartyId, CancellationToken cancellationToken = default);
	}
}
