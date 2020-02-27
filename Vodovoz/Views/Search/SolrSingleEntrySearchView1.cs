using System;
using System.Linq;
using Gdk;
using Vodovoz.SearchViewModels;
using Gtk;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Views.Search
{
	/*
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SolrSingleEntrySearchView1 : Gtk.Bin
	{
		private readonly SingleEntrySolrCriterionSearchViewModel viewModel;

		public SolrSearchCompletionWindow SearchCompletionView { get; set; }

		public SolrSingleEntrySearchView1(SingleEntrySolrCriterionSearchViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			SearchCompletionView = new SolrSearchCompletionWindow(viewModel.CompletionViewModel);
			viewModel.CompletionViewModel.PropertyChanged += CompletionViewModel_PropertyChanged;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			buttonAddAnd.Clicked += OnButtonAddAndClicked;
			buttonSearchClear.Clicked += OnButtonSearchClearClicked;

			entrySearch1.Binding.AddBinding(viewModel, vm => vm.SearchValue1, w => w.Text).InitializeFromSource();
			entrySearch1.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch1.FocusInEvent += EntrySearch_FocusInEvent;
			entrySearch1.FocusOutEvent += EntrySearch_FocusOutEvent;
			searchResult1.Binding.AddBinding(viewModel, vm => vm.SearchResult1, w => w.LabelProp).InitializeFromSource();
			searchResult1.Binding.AddBinding(viewModel, vm => vm.SearchResult1Visible, w => w.Visible).InitializeFromSource();

			entrySearch2.Binding.AddBinding(viewModel, vm => vm.SearchValue2, w => w.Text).InitializeFromSource();
			entrySearch2.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch2.FocusInEvent += EntrySearch_FocusInEvent;
			entrySearch2.FocusOutEvent += EntrySearch_FocusOutEvent;
			searchResult2.Binding.AddBinding(viewModel, vm => vm.SearchResult2, w => w.LabelProp).InitializeFromSource();
			searchResult2.Binding.AddBinding(viewModel, vm => vm.SearchResult2Visible, w => w.Visible).InitializeFromSource();

			entrySearch3.Binding.AddBinding(viewModel, vm => vm.SearchValue3, w => w.Text).InitializeFromSource();
			entrySearch3.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch3.FocusInEvent += EntrySearch_FocusInEvent;
			entrySearch3.FocusOutEvent += EntrySearch_FocusOutEvent;
			searchResult3.Binding.AddBinding(viewModel, vm => vm.SearchResult3, w => w.LabelProp).InitializeFromSource();
			searchResult3.Binding.AddBinding(viewModel, vm => vm.SearchResult3Visible, w => w.Visible).InitializeFromSource();

			entrySearch4.Binding.AddBinding(viewModel, vm => vm.SearchValue4, w => w.Text).InitializeFromSource();
			entrySearch4.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch4.FocusInEvent += EntrySearch_FocusInEvent;
			entrySearch4.FocusOutEvent += EntrySearch_FocusOutEvent;
			searchResult4.Binding.AddBinding(viewModel, vm => vm.SearchResult4, w => w.LabelProp).InitializeFromSource();
			searchResult4.Binding.AddBinding(viewModel, vm => vm.SearchResult4Visible, w => w.Visible).InitializeFromSource();

			SearchVisible(1);
		}

		void CompletionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(viewModel.CompletionViewModel.Visible):
					UpdateCompletionVisibility();
					break;
				default:
					break;
			}
		}

		//void SearchCompletionView_FocusOutEvent(object o, Gtk.FocusOutEventArgs args)
		//{
		//	viewModel.CompletionViewModel.VisibleWhileFocused = false;
		//}

		private void UpdateCompletionVisibility()
		{
			if(viewModel.CompletionViewModel.Visible) {
				ShowCompletion();
			} else {
				HideCompletion();
			}
		}

		private void ShowCompletion()
		{
			WindowStateChangeSubscribeIfNotSubscribed();
			Gtk.Application.Invoke((sender, e) => {
				GetCompletionPosition(entrySearch1, out int x, out int y);
				SearchCompletionView.Move(x, y);
				SearchCompletionView.ShowAll();
			});
		}

		private void HideCompletion()
		{
			Gtk.Application.Invoke((sender, e) => {
				SearchCompletionView.Hide();
			});
		}

		#region Определение позиции Completion окна

		Gtk.Window window;

		private void WindowStateChangeSubscribeIfNotSubscribed()
		{
			//Поиск Gtk окна только при первой отрисовке
			if(window != null) {
				return;
			}
			Widget parent = Parent;
			while(!(parent is Gtk.Window) && parent != null) {
				parent = parent.Parent;
			}
			window = parent as Gtk.Window;
			if(window != null) {
				window.WindowStateEvent += Window_WindowStateEvent;
			}
		}

		void Window_WindowStateEvent(object o, WindowStateEventArgs args)
		{
			//Костыль. При переходе окна в Maximized режим, при первой отрисовке, 
			//неправльно считываются позиции окна, из-за этого приходится ждать 
			//некоторое время чтобы позицию получилось считать правильно
			if(args.Event.NewWindowState == WindowState.Maximized) {
				Task.Delay(5).ContinueWith((task) => {
					UpdateCompletionVisibility();
				});
			} else {
				UpdateCompletionVisibility();
			}
		}

		int? topOffset = null;
		int? leftOffset = null;

		private void GetCompletionPosition(Widget widget, out int x, out int y)
		{
			window.GetPosition(out int winX, out int winY);
			widget.ParentWindow.GetPosition(out int gdkWinX, out int gdkWinY);
			//Костыль. Расчет ширины левой границы окна и заголовка окна, 
			//расчитывается правильно только при первой прорисовке, в 
			//дальнейшем он очень часто может расчитываться неверно, 
			//из-за этого запоминаем эти величины при первой отрисовке
			if(topOffset == null) {
				topOffset = gdkWinY - winY;
			}
			if(leftOffset == null) {
				leftOffset = gdkWinX - winX;
			}

			x = widget.Allocation.X + winX + leftOffset.Value;
			y = widget.Allocation.Y + widget.Allocation.Height + 1 + winY + topOffset.Value;
		}

		#endregion Определение позиции Completion окна



		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			viewModel.ClearSearchValues();
		}

		private int searchEntryShown = 1;

		protected void OnButtonAddAndClicked(object sender, EventArgs e)
		{
			SearchVisible(searchEntryShown + 1);
		}

		private void SearchVisible(int count)
		{
			entrySearch1.Visible = count > 0;
			ylabelSearchAnd.Visible = entrySearch2.Visible = count > 1;
			ylabelSearchAnd2.Visible = entrySearch3.Visible = count > 2;
			ylabelSearchAnd3.Visible = entrySearch4.Visible = count > 3;
			buttonAddAnd.Sensitive = count < 4;
			searchEntryShown = count;
		}

		void EntrySearch_FocusInEvent(object o, FocusInEventArgs args)
		{
			//viewModel.CompletionViewModel.VisibleForSearchEntry = true;
		}

		void EntrySearch_FocusOutEvent(object o, FocusOutEventArgs args)
		{
			viewModel.CompletionViewModel.VisibleForSearchEntry = false;
		}

		protected void EntrySearch_WidgetEvent(object o, Gtk.WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					viewModel.ManualSearchUpdate();
				}

				if(eventKey != null && (eventKey.Key == Gdk.Key.Down || eventKey.Key == Gdk.Key.KP_Down)) {
					SearchCompletionView.GrabFocus();
				}
			}
		}

		private void DestroyCompletion()
		{
			//SearchCompletionView.FocusOutEvent -= SearchCompletionView_FocusOutEvent;
			SearchCompletionView.Destroy();
		}

		protected override void OnDestroyed()
		{
			DestroyCompletion();
			base.OnDestroyed();
		}
	}*/
}
