using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameter;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModel
{
	public class DeliveryTimeAdvancedWageParameterViewModel : EntityTabViewModelBase<DeliveryTimeAdvancedWageParameter>
	{
		public DeliveryTimeAdvancedWageParameterViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
		}

		public DeliveryTimeAdvancedWageParameterViewModel(IUnitOfWorkGeneric<DeliveryTimeAdvancedWageParameter> uow, ICommonServices commonServices) : base(uow, commonServices)
		{
		}
	}
}
