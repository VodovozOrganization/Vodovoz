using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModels;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels
{
	public class DeliveryTimeAdvancedWageParameterViewModel : EntityWidgetViewModelBase<DeliveryTimeAdvancedWageParameter> , IWageParameterViewModel
	{
		public DeliveryTimeAdvancedWageParameterViewModel(DeliveryTimeAdvancedWageParameter entity, ICommonServices commonServices) : base(entity, commonServices)
		{
		}

		public AdvancedWageParameter GetParameter()
		{
			return Entity as AdvancedWageParameter;
		}
	}
}
