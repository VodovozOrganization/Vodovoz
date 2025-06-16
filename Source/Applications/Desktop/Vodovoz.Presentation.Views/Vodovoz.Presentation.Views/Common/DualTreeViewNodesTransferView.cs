using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views.Common
{
	[ToolboxItem(true)]
	public partial class DualTreeViewNodesTransferView
		: WidgetViewBase<DualTreeViewNodesTransferViewModel>
	{
		public DualTreeViewNodesTransferView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel == null)
			{
				return;
			}

			base.ConfigureWidget();

			if(!ViewModel.SkipTreeViewConfig)
			{
				ytreeviewLeft.CreateFluentColumnsConfig<object>()
					.AddColumn("")
					.AddTextRenderer(x => ViewModel.ItemLeftDisplayFunc.Invoke(x))
					.Finish();
			}

			ytreeviewLeft.ItemsDataSource = ViewModel.LeftItems;
			ytreeviewLeft.Selection.Mode = SelectionMode.Multiple;
			ytreeviewLeft.HeadersVisible = false;
			ytreeviewLeft.Binding
				.AddBinding(
					ViewModel,
					x => x.SelectedLeftItems,
					w => w.SelectedRows)
				.InitializeFromSource();

			if(!ViewModel.SkipTreeViewConfig)
			{
				ytreeviewRight.CreateFluentColumnsConfig<object>()
					.AddColumn("")
					.AddTextRenderer(x => ViewModel.ItemRightDisplayFunc.Invoke(x))
					.Finish();
			}

			ytreeviewRight.ItemsDataSource = ViewModel.RightItems;
			ytreeviewRight.Selection.Mode = SelectionMode.Multiple;
			ytreeviewRight.HeadersVisible = false;
			ytreeviewRight.Binding
				.AddBinding(
					ViewModel,
					x => x.SelectedRightItems,
					w => w.SelectedRows)
				.InitializeFromSource();

			buttonToLeft.Clicked += OnMoveToLeftButtonClicked;
			buttonToRight.Clicked += OnMoveToRightButtonClicked;
			buttonToLeftAll.Clicked += OnMoveAllToLeftButtonClicked;
			buttonToRightAll.Clicked += OnMoveAllToRightButtonClicked;

			buttonToLeftAll.Visible = ViewModel.ShowAllButtons;
			buttonToRightAll.Visible = ViewModel.ShowAllButtons;

			EntitySearch.Visible = ViewModel.IsSearchVisible;

			if(EntitySearch.ViewModel != null)
			{
				EntitySearch.ViewModel.PropertyChanged += OnSearchTextChanged;
			}
		}

		public yTreeView YTreeviewLeft => ytreeviewLeft;

		public yTreeView YTreeViewRight => ytreeviewRight;

		private void OnSearchTextChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.SearchText))
			{
				ytreeviewLeft.SearchHighlightText = ViewModel.SearchText;
				ytreeviewRight.SearchHighlightText = ViewModel.SearchText;
			}
		}

		private void OnMoveToLeftButtonClicked(object sender, EventArgs e)
		{
			ViewModel.MoveToLeftCommand.Execute(null);
		}

		private void OnMoveToRightButtonClicked(object sender, EventArgs e)
		{
			ViewModel.MoveToRightCommand.Execute(null);
		}

		private void OnMoveAllToLeftButtonClicked(object sender, EventArgs e)
		{
			ViewModel.MoveAllToLeftCommand.Execute(null);
		}

		private void OnMoveAllToRightButtonClicked(object sender, EventArgs e)
		{
			ViewModel.MoveAllToRightCommand.Execute(null);
		}
	}
}
