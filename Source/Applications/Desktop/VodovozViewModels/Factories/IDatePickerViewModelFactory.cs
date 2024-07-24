using System;
using Vodovoz.ViewModels.Widgets.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public interface IDatePickerViewModelFactory
	{
		DatePickerViewModel CreateNewDatePickerViewModel(
			DateTime dateTime,
			ChangeDateType changeDateType = ChangeDateType.Month,
			Func<DateTime, bool> canSelectNextMonthFunc = null,
			Func<DateTime, bool> canSelectPreviousMonthFunc = null);
	}
}
