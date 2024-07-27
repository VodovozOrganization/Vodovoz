using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using Image = Gtk.Image;

namespace Vodovoz.ViewWidgets.Profitability
{
	[ToolboxItem(true)]
	public partial class MonthPickerView : WidgetViewBase<DatePickerViewModel>
	{
		public MonthPickerView(DatePickerViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnNextDate.BindCommand(ViewModel.NextDateCommand);
			btnPreviousDate.BindCommand(ViewModel.PreviousDateCommand);

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
	}
}
