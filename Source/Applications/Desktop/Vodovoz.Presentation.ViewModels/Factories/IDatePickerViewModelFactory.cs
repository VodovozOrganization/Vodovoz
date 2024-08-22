using System;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;

namespace Vodovoz.Presentation.ViewModels.Factories
{
	public interface IDatePickerViewModelFactory
	{
		DatePickerViewModel CreateNewDatePickerViewModel(
			DateTime dateTime,
			ChangeDateType changeDateType = ChangeDateType.Month,
			Func<DateTime, bool> canSelectNextDateFunc = null,
			Func<DateTime, bool> canSelectPreviousDateFunc = null);
	}
}
