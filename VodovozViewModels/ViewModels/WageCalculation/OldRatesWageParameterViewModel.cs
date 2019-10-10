using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class OldRatesWageParameterViewModel : EntityWidgetViewModelBase<OldRatesWageParameter>
	{
		public OldRatesWageParameterViewModel(OldRatesWageParameter entity, ICommonServices commonServices) : base(entity, commonServices)
		{
		}
	}
}
