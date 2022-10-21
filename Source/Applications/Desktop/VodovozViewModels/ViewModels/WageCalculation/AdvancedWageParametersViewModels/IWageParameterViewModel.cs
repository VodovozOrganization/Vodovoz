using System;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModels
{
	public interface IWageParameterViewModel
	{
		AdvancedWageParameter GetParameter();
	}
}
