using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelPricesUpdateService
	{
		Task UpdateFuelPricesByLastWeekTransaction(CancellationToken cancellationToken);
	}
}
