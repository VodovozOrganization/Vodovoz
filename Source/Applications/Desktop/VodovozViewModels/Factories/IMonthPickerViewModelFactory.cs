using System;
using Vodovoz.ViewModels.Widgets.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public interface IMonthPickerViewModelFactory
	{
		MonthPickerViewModel CreateNewMonthPickerViewModel(
			DateTime dateTime,
			Func<DateTime, bool> canSelectNextMonthFunc = null,
			Func<DateTime, bool> canSelectPreviousMonthFunc = null);
	}
}
