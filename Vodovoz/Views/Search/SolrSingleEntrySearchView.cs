using System;
using System.Linq;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using Vodovoz.SearchViewModels;

namespace Vodovoz.Views.Search
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SolrSingleEntrySearchView : Gtk.Bin
	{
		private readonly SingleEntrySolrCriterionSearchViewModel viewModel;

		public SolrSearchCompletionWindow SearchCompletionView { get; set; }


		public SolrSingleEntrySearchView(SingleEntrySolrCriterionSearchViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			SearchCompletionView = new SolrSearchCompletionWindow(viewModel.CompletionViewModel);

			viewModel.CompletionViewModel.PropertyChanged += CompletionViewModel_PropertyChanged;

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			buttonSearchClear.Clicked += ButtonSearchClear_Clicked;;

			entrySearch.Binding.AddBinding(viewModel, vm => vm.SearchValue, w => w.Text).InitializeFromSource();
			entrySearch.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch.FocusInEvent += EntrySearch_FocusInEvent;;
			entrySearch.FocusOutEvent += EntrySearch_FocusOutEvent;;
			searchResult.Binding.AddBinding(viewModel, vm => vm.SearchResult, w => w.LabelProp).InitializeFromSource();
			searchResult.Binding.AddBinding(viewModel, vm => vm.SearchResultVisible, w => w.Visible).InitializeFromSource();
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
				GetCompletionPosition(entrySearch, out int x, out int y);
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


		void ButtonSearchClear_Clicked(object sender, EventArgs e)
		{
			viewModel.Clear();
		}

		void EntrySearch_FocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			//viewModel.CompletionViewModel.VisibleForSearchEntry = true;
		}

		void EntrySearch_FocusOutEvent(object o, Gtk.FocusOutEventArgs args)
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
			SearchCompletionView.Destroy();
		}

		protected override void OnDestroyed()
		{
			DestroyCompletion();
			base.OnDestroyed();
		}
	}
}
