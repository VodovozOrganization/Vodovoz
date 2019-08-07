using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Complaints;
using Gtk;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintDiscussionsView : EntityWidgetViewBase<ComplaintDiscussionsViewModel>
	{
		public ComplaintDiscussionsView(ComplaintDiscussionsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private VBox complaintDiscussionViewsBox;

		private void ConfigureDlg()
		{
			ybuttonAttachSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanAttachSubdivision, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachSubdivision.Clicked += (sender, e) => ViewModel.AttachSubdivisionCommand.Execute();

			ViewModel.ObservableComplaintDiscussionViewModels.ListChanged += (aList) => GenerateDiscussionViews();

			GenerateDiscussionViews();
		}

		private void GenerateDiscussionViews()
		{
			if(complaintDiscussionViewsBox != null) {
				complaintDiscussionViewsBox.Destroy();
			}
			complaintDiscussionViewsBox = new VBox();

			foreach(ComplaintDiscussionViewModel vm in ViewModel.ObservableComplaintDiscussionViewModels) {
				var view = new ComplaintDiscussionView(vm);
				complaintDiscussionViewsBox.Add(view);
			}

			vboxSubdivisionItems.Add(complaintDiscussionViewsBox);
			vboxSubdivisionItems.ShowAll();
		}
	}
}