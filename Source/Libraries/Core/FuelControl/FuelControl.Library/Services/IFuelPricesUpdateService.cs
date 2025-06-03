using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelPricesUpdateService
	{
		/// <summary>
		/// Обновление стоимости топлива по данным транзакций за предыдущую неделю
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		Task UpdateFuelPricesByLastWeekTransaction(CancellationToken cancellationToken);
	}
}
