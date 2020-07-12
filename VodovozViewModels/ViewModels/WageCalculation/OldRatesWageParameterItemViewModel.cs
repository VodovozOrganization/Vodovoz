using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class OldRatesWageParameterItemViewModel : EntityWidgetViewModelBase<OldRatesWageParameterItem>
	{
		public OldRatesWageParameterItemViewModel(OldRatesWageParameterItem entity, ICommonServices commonServices) : base(entity, commonServices)
		{
		}
	}
}
