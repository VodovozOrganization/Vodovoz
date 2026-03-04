using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.Repositories.Cash
{
	public interface IVatRateRepository
	{
		/// <summary>
		/// Получить ставку НДС по значению
		/// </summary>
		/// <returns>Ставка НДС</returns>
		VatRate GetVatRateByValue(IUnitOfWork unitOfWork, decimal vatRateValue);
	}
}
