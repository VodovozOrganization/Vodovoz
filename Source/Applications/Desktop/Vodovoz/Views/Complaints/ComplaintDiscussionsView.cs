using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintDiscussionsView : WidgetViewBase<ComplaintDiscussionsViewModel>
	{
		public ComplaintDiscussionsView(ComplaintDiscussionsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			ybuttonAttachSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanAttachSubdivision, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachSubdivision.Clicked += (sender, e) => ViewModel.AttachSubdivisionCommand.Execute();

			ybuttonAttachSubdivisionByComplaintKind.Binding.AddBinding(ViewModel, vm => vm.CanAttachSubdivision, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachSubdivisionByComplaintKind.Clicked += (sender, e) => ViewModel.AttachSubdivisionByComplaintKindCommand.Execute();

			ViewModel.ObservableComplaintDiscussionViewModels.ElementAdded += ObservableComplaintDiscussionViewModels_ElementAdded;
			ViewModel.ObservableComplaintDiscussionViewModels.ElementRemoved += ObservableComplaintDiscussionViewModels_ElementRemoved;

			GenerateDiscussionViews();
		}

		void ObservableComplaintDiscussionViewModels_ElementAdded(object aList, int[] aIdx)
		{
			GenerateDiscussionViews();
			notebookDiscussions.CurrentPage = notebookDiscussions.NPages - 1;
		}

		void ObservableComplaintDiscussionViewModels_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			GenerateDiscussionViews();
		}

		Notebook notebookDiscussions;

		private string GetTabName(ComplaintDiscussionViewModel discussionVM)
		{
			string tabColor;
			switch(discussionVM.Entity.Status) {
				case Domain.Complaints.ComplaintDiscussionStatuses.Checking:
					tabColor = GdkColors.SuccessText.ToHtmlColor();
					break;
				case Domain.Complaints.ComplaintDiscussionStatuses.Closed:
					tabColor = GdkColors.PrimaryText.ToHtmlColor();
					break;
				default:
					tabColor = GdkColors.DangerText.ToHtmlColor();
					break;
			}
			return $"<span foreground = '{tabColor}'><b>{discussionVM.SubdivisionShortName}</b></span>";
		}

		private void GenerateDiscussionViews()
		{
			if(notebookDiscussions != null) {
				notebookDiscussions.Destroy();
			}
			notebookDiscussions = new Notebook();

			foreach(ComplaintDiscussionViewModel vm in ViewModel.ObservableComplaintDiscussionViewModels) {
				var view = new ComplaintDiscussionView(vm);
				VBox complaintDiscussionViewsBox = new VBox();
				complaintDiscussionViewsBox.Add(view);
				Box.BoxChild viewBox = (Box.BoxChild)complaintDiscussionViewsBox[view];
				viewBox.Fill = true;
				viewBox.Expand = true;
				var scrolledWindow = new ScrolledWindow();
				scrolledWindow.Add(complaintDiscussionViewsBox);

				Label tabLabel = new Label() {
					UseMarkup = true,
					Markup = GetTabName(vm)
				};

				vm.Entity.PropertyChanged -= Vm_PropertyChanged;
				vm.Entity.PropertyChanged += Vm_PropertyChanged;

				void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
				{
					if(e.PropertyName == nameof(vm.Entity.Status)) {
						if(tabLabel == null) {
							vm.Entity.PropertyChanged += Vm_PropertyChanged;
							return;
						}

						tabLabel.Markup = GetTabName(vm);
					}
				}

				notebookDiscussions.AppendPage(scrolledWindow, tabLabel);
			}

			vboxSubdivisionItems.Add(notebookDiscussions);
			vboxSubdivisionItems.ShowAll();
		}
	}
}
