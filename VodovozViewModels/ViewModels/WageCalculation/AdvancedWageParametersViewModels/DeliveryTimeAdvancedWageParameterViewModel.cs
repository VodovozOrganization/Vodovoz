using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels
{
	public class DeliveryTimeAdvancedWageParameterViewModel : EntityWidgetViewModelBase<DeliveryTimeAdvancedWageParameter>
	{
		public DeliveryTimeAdvancedWageParameterViewModel(DeliveryTimeAdvancedWageParameter entity, ICommonServices commonServices) : base(entity, commonServices)
		{
		}
	}
}
