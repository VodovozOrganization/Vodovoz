using System;
using QS.ViewModels;
using QS.Commands;

namespace Vodovoz.ViewModels.Widgets.Profitability
{
	public class MonthPickerViewModel : WidgetViewModelBase
	{
		private DelegateCommand _nextMonthCommand;
		private DelegateCommand _previousMonthCommand;
		private DateTime _selectedMonth;

		private readonly Func<DateTime, bool> _canSelectNextMonthFunc;
		private readonly Func<DateTime, bool> _canSelectPreviousMonthFunc;

		public MonthPickerViewModel(
			DateTime selectedMonth,
			Func<DateTime, bool> canSelectNextMonthFunc = null,
			Func<DateTime, bool> canSelectPreviousMonthFunc = null)
		{
			SetSelectedMonth(selectedMonth);
			_canSelectNextMonthFunc = canSelectNextMonthFunc;
			_canSelectPreviousMonthFunc = canSelectPreviousMonthFunc;
		}

		public DateTime SelectedMonth
		{
			get => _selectedMonth;
			set
			{
				if(SetField(ref _selectedMonth, value))
				{
					OnPropertyChanged(nameof(SelectedMonthTitle));
					UpdateState();
				}
			}
		}

		public void UpdateState()
		{
			OnPropertyChanged(nameof(CanSelectNextMonth));
			OnPropertyChanged(nameof(CanSelectPreviousMonth));
		}

		public string SelectedMonthTitle => SelectedMonth.ToString("Y");
		public bool CanSelectNextMonth => _canSelectNextMonthFunc == null || _canSelectNextMonthFunc.Invoke(SelectedMonth);
		public bool CanSelectPreviousMonth => _canSelectPreviousMonthFunc == null || _canSelectPreviousMonthFunc.Invoke(SelectedMonth);

		public DelegateCommand NextMonthCommand => _nextMonthCommand ?? (_nextMonthCommand = new DelegateCommand(
			() =>
			{
				SelectedMonth = SelectedMonth.AddMonths(1);
			}
		));

		public DelegateCommand PreviousMonthCommand => _previousMonthCommand ?? (_previousMonthCommand = new DelegateCommand(
			() =>
			{
				SelectedMonth = SelectedMonth.AddMonths(-1);
			}
		));

		private void SetSelectedMonth(DateTime selectedMonth)
		{
			SelectedMonth = selectedMonth.Day != 1
				? new DateTime(selectedMonth.Year, selectedMonth.Month, 1)
				: selectedMonth;
		}
	}
}
