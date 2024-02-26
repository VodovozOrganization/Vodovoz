using QS.Views;
using Vodovoz.Footers.ViewModels;

namespace Vodovoz.Footers.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BusinessTasksJournalFooterView : ViewBase<BusinessTasksJournalFooterViewModel>
	{
		public BusinessTasksJournalFooterView(BusinessTasksJournalFooterViewModel viewModel)
			:base(viewModel)
		{
			this.Build();
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
