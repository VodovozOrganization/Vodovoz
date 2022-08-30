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

		private readonly Func<bool> _canSelectNextMonthFunc;
		private readonly Func<bool> _canSelectPreviousMonthFunc;

		public MonthPickerViewModel(
			DateTime selectedMonth,
			Func<bool> canSelectNextMonthFunc = null,
			Func<bool> canSelectPreviousMonthFunc = null)
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
					OnPropertyChanged(nameof(CanSelectNextMonth));
					OnPropertyChanged(nameof(CanSelectPreviousMonth));
				}
			}
		}

		public string SelectedMonthTitle => SelectedMonth.ToString("Y");

		public bool CanSelectNextMonth
		{
			get
			{
				if(_canSelectNextMonthFunc != null)
				{
					return _canSelectNextMonthFunc.Invoke();
				}
				return true;
			}
		}

		public bool CanSelectPreviousMonth
		{
			get
			{
				if(_canSelectPreviousMonthFunc != null)
				{
					return _canSelectPreviousMonthFunc.Invoke();
				}
				return true;
			}
		}

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
			if(selectedMonth.Day != 1)
			{
				SelectedMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
			}
			else
			{
				SelectedMonth = selectedMonth;
			}
		}
	}
}
