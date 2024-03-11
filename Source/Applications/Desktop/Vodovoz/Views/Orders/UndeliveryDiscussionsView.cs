using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveryDiscussionsView : WidgetViewBase<UndeliveryDiscussionsViewModel>
	{
		private	Notebook _notebookDiscussions;
		public UndeliveryDiscussionsView(UndeliveryDiscussionsViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			ybuttonAttachSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanAttachSubdivision, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachSubdivision.Clicked += (sender, e) => ViewModel.AttachSubdivisionCommand.Execute();

			ViewModel.ObservableUndeliveryDiscussionViewModels.ElementAdded += ObservableUndeliveryDiscussionViewModels_ElementAdded;
			ViewModel.ObservableUndeliveryDiscussionViewModels.ElementRemoved += ObservableUndeliveryDiscussionViewModels_ElementRemoved;

			GenerateDiscussionViews();
		}

		private void ObservableUndeliveryDiscussionViewModels_ElementAdded(object aList, int[] aIdx)
		{
			GenerateDiscussionViews();
			_notebookDiscussions.CurrentPage = _notebookDiscussions.NPages - 1;
		}

		private void ObservableUndeliveryDiscussionViewModels_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			GenerateDiscussionViews();
		}

		private string GetTabName(UndeliveryDiscussionViewModel discussionVM)
		{
			string tabColor;

			switch(discussionVM.Entity.Status)
			{
				case Domain.Orders.UndeliveryDiscussionStatus.Checking:
					tabColor = GdkColors.SuccessText.ToHtmlColor();
					break;
				case Domain.Orders.UndeliveryDiscussionStatus.Closed:
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
			if(_notebookDiscussions != null)
			{
				_notebookDiscussions.Destroy();
			}

			_notebookDiscussions = new Notebook();

			foreach(UndeliveryDiscussionViewModel vm in ViewModel.ObservableUndeliveryDiscussionViewModels)
			{
				var view = new UndeliveryDiscussionView(vm);
				VBox complaintDiscussionViewsBox = new VBox();
				complaintDiscussionViewsBox.Add(view);
				Box.BoxChild viewBox = (Box.BoxChild)complaintDiscussionViewsBox[view];
				viewBox.Fill = true;
				viewBox.Expand = true;
				var scrolledWindow = new ScrolledWindow();
				scrolledWindow.Add(complaintDiscussionViewsBox);

				Label tabLabel = new Label()
				{
					UseMarkup = true,
					Markup = GetTabName(vm)
				};

				vm.Entity.PropertyChanged -= OnUndeliveryDiscussionViewModelPropertyChanged;
				vm.Entity.PropertyChanged += OnUndeliveryDiscussionViewModelPropertyChanged;

				void OnUndeliveryDiscussionViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
				{
					if(e.PropertyName == nameof(vm.Entity.Status))
					{
						if(tabLabel == null)
						{
							vm.Entity.PropertyChanged += OnUndeliveryDiscussionViewModelPropertyChanged;
							return;
						}

						tabLabel.Markup = GetTabName(vm);
					}
				}

				_notebookDiscussions.AppendPage(scrolledWindow, tabLabel);
			}

			vboxSubdivisionItems.Add(_notebookDiscussions);
			vboxSubdivisionItems.ShowAll();
		}
	}
}
