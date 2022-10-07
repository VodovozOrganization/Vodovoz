using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.ViewModels.ViewModels.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public interface IProfitabilityConstantsDataViewModelFactory
	{
		ProfitabilityConstantsDataViewModel CreateProfitabilityConstantsDataViewModel(IUnitOfWork uow, ProfitabilityConstants entity);
	}
}
