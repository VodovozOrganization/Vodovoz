using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.ViewModels.ViewModels.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public class ProfitabilityConstantsDataViewModelFactory : IProfitabilityConstantsDataViewModelFactory
	{
		private readonly ISelectableParametersFilterViewModelFactory _selectableParametersFilterViewModelFactory;

		public ProfitabilityConstantsDataViewModelFactory(
			ISelectableParametersFilterViewModelFactory selectableParametersFilterViewModelFactory)
		{
			_selectableParametersFilterViewModelFactory =
				selectableParametersFilterViewModelFactory
					?? throw new ArgumentNullException(nameof(selectableParametersFilterViewModelFactory));
		}
		
		public ProfitabilityConstantsDataViewModel CreateProfitabilityConstantsDataViewModel(
			IUnitOfWork uow,
			ProfitabilityConstants entity)
		{
			return new ProfitabilityConstantsDataViewModel(uow, entity, _selectableParametersFilterViewModelFactory);
		}
	}
}
