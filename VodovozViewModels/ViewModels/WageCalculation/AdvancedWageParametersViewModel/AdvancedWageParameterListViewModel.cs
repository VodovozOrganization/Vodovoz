using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModel
{
	public class AdvancedWageParameterListViewModel : UoWTabViewModelBase
	{
		public IList<AdvancedWageParameter> RootParameters { get; }

		public IAdvancedWageRepository AdvancedWageRepository { get; }

		public AdvancedWageParameterListViewModel(WageRate wageRate, IAdvancedWageRepository advancedWageRepository, IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService) : base(unitOfWorkFactory, interactiveService)
		{
			AdvancedWageRepository = advancedWageRepository ?? throw new ArgumentNullException(nameof(advancedWageRepository));
			if(unitOfWorkFactory == null)
				throw new ArgumentNullException(nameof(unitOfWorkFactory));

			UoW = unitOfWorkFactory.CreateWithoutRoot();
			RootParameters = AdvancedWageRepository.GetRootParameter(UoW, wageRate).ToList() ?? new List<AdvancedWageParameter>();
		}

	}
}
