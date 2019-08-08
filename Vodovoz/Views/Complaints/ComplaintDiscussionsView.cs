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

			ViewModel.ObservableComplaintDiscussionViewModels.ElementAdded += ObservableComplaintDiscussionViewModels_ElementAdded;
			ViewModel.ObservableComplaintDiscussionViewModels.ElementRemoved += ObservableComplaintDiscussionViewModels_ElementRemoved;

			GenerateDiscussionViews();
		}

		void ObservableComplaintDiscussionViewModels_ElementAdded(object aList, int[] aIdx)
		{
			GenerateDiscussionViews();
		}

		void ObservableComplaintDiscussionViewModels_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
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
				Box.BoxChild viewBox = (Box.BoxChild)complaintDiscussionViewsBox[view];
				viewBox.Fill = false;
				viewBox.Expand = false;
			}

			vboxSubdivisionItems.Add(complaintDiscussionViewsBox);
			vboxSubdivisionItems.ShowAll();
		}
	}
}