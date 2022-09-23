using System;
using Vodovoz.ViewModels.Widgets.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public class MonthPickerViewModelFactory : IMonthPickerViewModelFactory
	{
		public MonthPickerViewModel CreateNewMonthPickerViewModel(
			DateTime dateTime,
			Func<DateTime, bool> canSelectNextMonthFunc = null,
			Func<DateTime, bool> canSelectPreviousMonthFunc = null)
		{
			return new MonthPickerViewModel(dateTime, canSelectNextMonthFunc, canSelectPreviousMonthFunc);
		}
	}
}
