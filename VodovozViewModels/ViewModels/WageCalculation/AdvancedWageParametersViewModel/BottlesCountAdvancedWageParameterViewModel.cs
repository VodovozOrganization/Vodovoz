using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameter;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels
{
	public class BottlesCountAdvancedWageParameterViewModel : EntityTabViewModelBase<BottlesCountAdvancedWageParameter>
	{
		public BottlesCountAdvancedWageParameterViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
		}

		public BottlesCountAdvancedWageParameterViewModel(IUnitOfWorkGeneric<BottlesCountAdvancedWageParameter> uow, ICommonServices commonServices) : base(uow, commonServices)
		{
		}
	}
}
