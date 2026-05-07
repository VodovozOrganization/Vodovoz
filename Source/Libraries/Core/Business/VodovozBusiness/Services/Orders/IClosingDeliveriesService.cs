using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;

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
		Task CheckAndCloseDeliveriesAsync(IUnitOfWork unitOfWork, int? counterpartyId = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Открытие поставок у контрагента с отсутствующей просроченной задолженностью
		/// </summary>
		Task CheckAndOpenDeliveriesAsync(IUnitOfWork unitOfWork, Counterparty counterparty, CancellationToken cancellationToken = default);
	}
}
