using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[ToolboxItem(true)]
	public partial class ComplaintDiscussionsView : WidgetViewBase<ComplaintDiscussionsViewModel>
	{
		private Notebook _notebookDiscussions;

		public ComplaintDiscussionsView(ComplaintDiscussionsViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			_notebookDiscussions?.Destroy();
			_notebookDiscussions = new Notebook();

			vboxSubdivisionItems.Add(_notebookDiscussions);
			vboxSubdivisionItems.ShowAll();

			ybuttonAttachSubdivision.BindCommand(ViewModel.AttachSubdivisionCommand);
			ybuttonAttachSubdivisionByComplaintKind.BindCommand(ViewModel.AttachSubdivisionByComplaintKindCommand);

			ViewModel.ObservableComplaintDiscussionViewModels.ElementAdded += ObservableComplaintDiscussionViewModels_ElementAdded;
			ViewModel.ObservableComplaintDiscussionViewModels.ElementRemoved += ObservableComplaintDiscussionViewModels_ElementRemoved;

			GenerateDiscussionViews();
		}

		private void ObservableComplaintDiscussionViewModels_ElementAdded(object aList, int[] aIdx)
		{
			if(!(aList is IList<ComplaintDiscussionViewModel> list))
			{
				return;
			}

			foreach(var i in aIdx)
			{
				if(!(list[i] is ComplaintDiscussionViewModel complaintDiscossionViewModel))
				{
					continue;
				}

				CreateDiscussionPage(complaintDiscossionViewModel);
			}

			vboxSubdivisionItems.ShowAll();

			_notebookDiscussions.CurrentPage = _notebookDiscussions.NPages - 1;
		}

		private void ObservableComplaintDiscussionViewModels_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			vboxSubdivisionItems.HideAll();

			if(!(aList is IList<ComplaintDiscussionViewModel> list))
			{
				return;
			}

			foreach(var i in aIdx)
			{
				if(!(list[i] is ComplaintDiscussionViewModel complaintDiscossionViewModel))
				{
					continue;
				}

				RemoveDiscussionNotebookTab(complaintDiscossionViewModel);
			}

			vboxSubdivisionItems.ShowAll();
		}


		private void GenerateDiscussionViews()
		{
			foreach(ComplaintDiscussionViewModel viewModel in ViewModel.ObservableComplaintDiscussionViewModels)
			{
				CreateDiscussionPage(viewModel);
			}
		}

		private void CreateDiscussionPage(ComplaintDiscussionViewModel viewModel)
		{
			var view = new ComplaintDiscussionView(viewModel);
			var complaintDiscussionViewsBox = new VBox
			{
				view
			};

			var viewBox = (Box.BoxChild)complaintDiscussionViewsBox[view];
			viewBox.Fill = true;
			viewBox.Expand = true;

			var scrolledWindow = new ScrolledWindow
			{
				complaintDiscussionViewsBox
			};

			var tabLabel = new yLabel()
			{
				UseMarkup = true,
			};

			tabLabel.Binding
				.AddBinding(view, v => v.TabName, w => w.LabelProp)
				.InitializeFromSource();

			_notebookDiscussions.AppendPage(scrolledWindow, tabLabel);
		}

		private void RemoveDiscussionNotebookTab(ComplaintDiscussionViewModel complaintDiscossionViewModel)
		{
			var view = GetMatchingViewFromNotebook(complaintDiscossionViewModel);

			VBox parentVbox = view.Parent as VBox;
			parentVbox.Remove(view);
			view.Parent = null;
			view.Destroy();
			view.Dispose();

			ScrolledWindow parentScrollBox = parentVbox.Parent as ScrolledWindow;
			parentScrollBox.Remove(parentVbox);
			parentVbox.Parent = null;
			parentVbox.Destroy();
			parentVbox.Dispose();

			_notebookDiscussions.Remove(parentScrollBox);
			parentScrollBox.Parent = null;
			parentScrollBox.Destroy();
			parentScrollBox.Dispose();
		}

		private ComplaintDiscussionView GetMatchingViewFromNotebook(ComplaintDiscussionViewModel viewModel)
		{
			return _notebookDiscussions.Children
				.Cast<ScrolledWindow>()
				.SelectMany(x => x.Children)
				.Cast<VBox>()
				.SelectMany(x => x.Children)
				.Cast<ComplaintDiscussionView>()
				.FirstOrDefault(x => x.ViewModel == viewModel);
		}

		public override void Destroy()
		{
			ViewModel.ObservableComplaintDiscussionViewModels.ElementAdded -= ObservableComplaintDiscussionViewModels_ElementAdded;
			ViewModel.ObservableComplaintDiscussionViewModels.ElementRemoved -= ObservableComplaintDiscussionViewModels_ElementRemoved;

			_notebookDiscussions.Destroy();
			_notebookDiscussions.Dispose();
			_notebookDiscussions = null;

			base.Destroy();
		}
	}
}
