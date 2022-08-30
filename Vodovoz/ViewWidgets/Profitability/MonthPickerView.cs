using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.Profitability;

namespace Vodovoz.ViewWidgets.Profitability
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MonthPickerView : WidgetViewBase<MonthPickerViewModel>
	{
		public MonthPickerView(MonthPickerViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnNextMonth.Clicked += (sender, e) => ViewModel.NextMonthCommand.Execute();
			btnPreviousMonth.Clicked += (sender, e) => ViewModel.PreviousMonthCommand.Execute();

			btnNextMonth.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectNextMonth, w => w.Sensitive)
				.InitializeFromSource();
			btnPreviousMonth.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectPreviousMonth, w => w.Sensitive)
				.InitializeFromSource();

			entryMonth.Binding
				.AddBinding(ViewModel, vm => vm.SelectedMonthTitle, w => w.Text)
				.InitializeFromSource();
			entryMonth.IsEditable = false;
		}
	}
}
