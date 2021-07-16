using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;

namespace Vodovoz.JournalViewers.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverComplaintReasonJournalFilterView : FilterViewBase<DriverComplaintReasonJournalFilterViewModel>
	{
		public DriverComplaintReasonJournalFilterView(DriverComplaintReasonJournalFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			ycheckbuttonIsPopularOnly.Binding.AddBinding(ViewModel, vm => vm.IsPopular, w => w.Active).InitializeFromSource();
		}
	}
}
