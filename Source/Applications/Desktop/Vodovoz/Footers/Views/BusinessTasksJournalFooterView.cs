using QS.Views.GtkUI;
using Vodovoz.Footers.ViewModels;

namespace Vodovoz.Footers.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BusinessTasksJournalFooterView : Gtk.Bin
	{
		BusinessTasksJournalFooterViewModel ViewModel { get; set; }

		public BusinessTasksJournalFooterView(BusinessTasksJournalFooterViewModel viewModel)
		{
			this.Build();
			ViewModel = viewModel;
			Configure();
		}

		private void Configure()
		{
			labelRingCount.Binding.AddFuncBinding(ViewModel, vm => vm.RingCount.ToString(), v => v.Text).InitializeFromSource();
			labelHardClientsCount.Binding.AddFuncBinding(ViewModel, vm => vm.HardClientsCount.ToString(), v => v.Text).InitializeFromSource();
			labelTasksCount.Binding.AddFuncBinding(ViewModel, vm => vm.TasksCount.ToString(), v => v.Text).InitializeFromSource();
			labelTasks.Binding.AddFuncBinding(ViewModel, vm => vm.Tasks.ToString(), v => v.Text).InitializeFromSource();
			labelTareReturn.Binding.AddFuncBinding(ViewModel, vm => vm.TareReturn.ToString(), v => v.Text).InitializeFromSource();
		}
	}
}
