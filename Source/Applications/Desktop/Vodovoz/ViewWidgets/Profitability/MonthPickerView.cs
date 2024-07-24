using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.Profitability;

namespace Vodovoz.ViewWidgets.Profitability
{
	[System.ComponentModel.ToolboxItem(true)]
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

			entryDate.Binding
				.AddBinding(ViewModel, vm => vm.SelectedDateTitle, w => w.Text)
				.InitializeFromSource();
			entryDate.IsEditable = false;
		}
	}
}
