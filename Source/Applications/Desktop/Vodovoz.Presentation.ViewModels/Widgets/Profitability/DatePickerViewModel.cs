using System;
using QS.ViewModels;
using QS.Commands;

namespace Vodovoz.Presentation.ViewModels.Widgets.Profitability
{
	public class DatePickerViewModel : WidgetViewModelBase, IDisposable
	{
		private DateTime _selectedDate;
		
		private Func<DateTime, bool> _canSelectNextDateFunc;
		private Func<DateTime, bool> _canSelectPreviousDateFunc;

		public DatePickerViewModel(
			DateTime date,
			ChangeDateType changeDateType = ChangeDateType.Month,
			Func<DateTime, bool> canSelectNextMonthFunc = null,
			Func<DateTime, bool> canSelectPreviousMonthFunc = null)
		{
			ChangeDateType = changeDateType;
			_canSelectNextDateFunc = canSelectNextMonthFunc;
			_canSelectPreviousDateFunc = canSelectPreviousMonthFunc;
			
			CreateCommands();
			SetSelectedDate(date);
		}
		
		public event EventHandler DateChanged;
		public event EventHandler DateChangedByUser;

		public DateTime SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					OnDateChanged();
					OnPropertyChanged(nameof(SelectedDateTitle));
					UpdateState();
				}
			}
		}

		public void UpdateState()
		{
			OnPropertyChanged(nameof(CanSelectNextDate));
			OnPropertyChanged(nameof(CanSelectPreviousDate));
		}

		public string SelectedDateTitle
		{
			get
			{
				switch(ChangeDateType)
				{
					case ChangeDateType.Day:
						return SelectedDate.ToString("d");
					case ChangeDateType.Month:
						return SelectedDate.ToString("Y");
				}

				return "Неизвестный формат смены даты";
			}
		}

		public bool CanEditDateFromCalendar { get; set; } = false;
		public bool CanSelectNextDate => _canSelectNextDateFunc == null || _canSelectNextDateFunc.Invoke(SelectedDate);
		public bool CanSelectPreviousDate => _canSelectPreviousDateFunc == null || _canSelectPreviousDateFunc.Invoke(SelectedDate);
		public ChangeDateType ChangeDateType { get; }

		public DelegateCommand NextDateCommand { get; private set; }
		public DelegateCommand PreviousDateCommand { get; private set; }
		
		public bool CanChangeToNextDate(DateTime date)
			=> _canSelectNextDateFunc == null || _canSelectNextDateFunc.Invoke(date);
		public bool CanChangeToPreviousDate(DateTime date)
			=> _canSelectPreviousDateFunc == null || _canSelectPreviousDateFunc.Invoke(date);
		
		public virtual void OnDateChangedByUser()
		{
			DateChangedByUser?.Invoke(this, EventArgs.Empty);
		}
		
		protected virtual void OnDateChanged()
		{
			DateChanged?.Invoke(this, EventArgs.Empty);
		}

		private void CreateCommands()
		{
			NextDateCommand = new DelegateCommand(SetNextDate);
			PreviousDateCommand = new DelegateCommand(SetPreviousDate);
		}

		private void SetSelectedDate(DateTime selectedDate)
		{
			switch(ChangeDateType)
			{
				case ChangeDateType.Month:
					SelectedDate = selectedDate.Day != 1
						? new DateTime(selectedDate.Year, selectedDate.Month, 1)
						: selectedDate;
					break;
				case ChangeDateType.Day:
					SelectedDate = selectedDate;
					break;
			}
		}

		private void SetNextDate()
		{
			switch(ChangeDateType)
			{
				case ChangeDateType.Month:
					SelectedDate = SelectedDate.AddMonths(1);
					break;
				case ChangeDateType.Day:
					SelectedDate = SelectedDate.AddDays(1);
					break;
			}
			
			OnDateChangedByUser();
		}

		private void SetPreviousDate()
		{
			switch(ChangeDateType)
			{
				case ChangeDateType.Month:
					SelectedDate = SelectedDate.AddMonths(-1);
					break;
				case ChangeDateType.Day:
					SelectedDate = SelectedDate.AddDays(-1);
					break;
			}
			
			OnDateChangedByUser();
		}

		public void Dispose()
		{
			_canSelectNextDateFunc = null;
			_canSelectPreviousDateFunc = null;
		}
	}
}
