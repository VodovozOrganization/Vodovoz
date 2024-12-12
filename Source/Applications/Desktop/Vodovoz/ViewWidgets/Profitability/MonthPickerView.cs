using System;
using System.ComponentModel;
using Gtk;
using Pango;
using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using Image = Gtk.Image;

namespace Vodovoz.ViewWidgets.Profitability
{
	[ToolboxItem(true)]
	public partial class MonthPickerView : WidgetViewBase<DatePickerViewModel>
	{
		private Dialog _calendarDialog;
		public static int? CalendarFontSize;
		
		public MonthPickerView(DatePickerViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnNextDate.BindCommand(ViewModel.NextDateCommand);
			btnPreviousDate.BindCommand(ViewModel.PreviousDateCommand);
			btnCalendar.Clicked += OnCalendarClicked;

			btnNextDate.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectNextDate, w => w.Sensitive)
				.InitializeFromSource();
			btnPreviousDate.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectPreviousDate, w => w.Sensitive)
				.InitializeFromSource();
			
			btnCalendar.Image = new Image(typeof(Startup).Assembly, "Vodovoz.icons.common.Сalendar.png");
			btnCalendar.Binding
				.AddBinding(ViewModel, vm => vm.CanEditDateFromCalendar, w => w.Sensitive)
				.InitializeFromSource();

			entryDate.WidthRequest = 100;
			entryDate.Binding
				.AddBinding(ViewModel, vm => vm.SelectedDateTitle, w => w.Text)
				.InitializeFromSource();
			entryDate.IsEditable = false;
		}

		private void OnCalendarClicked(object sender, EventArgs e)
		{
			var parentWin = (Window)Toplevel;
			_calendarDialog = new Dialog(
				"Выберите дату",
				parentWin,
				DialogFlags.DestroyWithParent)
			{
				Modal = true
			};
			
			_calendarDialog.AddButton ("Отмена", ResponseType.Cancel);
			_calendarDialog.AddButton ("Ok", ResponseType.Ok);
			
			var calendar = new Calendar();
			calendar.DisplayOptions = CalendarDisplayOptions.ShowHeading  | 
			                          CalendarDisplayOptions.ShowDayNames | 
			                          CalendarDisplayOptions.ShowWeekNumbers;
			
			calendar.DaySelectedDoubleClick += OnCalendarDaySelectedDoubleClick;
			calendar.Date = ViewModel.SelectedDate;

			if(CalendarFontSize.HasValue)
			{
				var desc = new FontDescription { AbsoluteSize = CalendarFontSize.Value * 1000 };
				calendar.ModifyFont(desc);
			}
			_calendarDialog.VBox.Add(calendar);
			_calendarDialog.ShowAll();
			var response = _calendarDialog.Run();
			
			if(response == (int)ResponseType.Ok)
			{
				var selectedDate = calendar.GetDate();
				
				if(ViewModel.CanChangeToPreviousDate(selectedDate) && ViewModel.CanChangeToNextDate(selectedDate))
				{
					ViewModel.SelectedDate = selectedDate;
					ViewModel.OnDateChangedByUser();
				}
			}
			
			calendar.Destroy();
			_calendarDialog.Destroy();
		}
		
		private void OnCalendarDaySelectedDoubleClick(object sender, EventArgs e)
		{
			if(sender is Calendar calendar)
			{
				calendar.DaySelectedDoubleClick -= OnCalendarDaySelectedDoubleClick;
			}
			
			_calendarDialog.Respond(ResponseType.Ok);
		}
	}
}
