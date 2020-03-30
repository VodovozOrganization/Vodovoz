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
			viewModel.PropertyChanged += ViewModel_PropertyChanged;

			Shown += SolrSingleEntrySearchView_Shown;
			ParentSet += SolrSingleEntrySearchView_ParentSet;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			buttonSearchClear.Clicked += ButtonSearchClear_Clicked; ;
			ybuttonHelp.Clicked += (sender, e) => new SolrSearchHelpWindow().Show();

			entrySearch.Binding.AddBinding(viewModel, vm => vm.SearchValue, w => w.Text).InitializeFromSource();
			entrySearch.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch.FocusInEvent += EntrySearch_FocusInEvent;
			entrySearch.FocusOutEvent += EntrySearch_FocusOutEvent;
			searchResult.Binding.AddBinding(viewModel, vm => vm.SearchResult, w => w.LabelProp).InitializeFromSource();
			searchResult.Binding.AddBinding(viewModel, vm => vm.SearchResultVisible, w => w.Visible).InitializeFromSource();
		}

		/// <summary>
		/// Необходимо для удаления ссылки на Source у биндинга, 
		/// в случае если Source оставался тем же объектом, 
		/// а биндинги несколько раз пересоздавались, 
		/// например при пересоздании View для той же самой ViewModel
		/// </summary>
		private void ClearBindings()
		{
			entrySearch.Binding.CleanSources();
			searchResult.Binding.CleanSources();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(viewModel.SolrUnavailable):
					UpdateWarningMessage();
					break;
				default:
					break;
			}
		}

		private void UpdateWarningMessage()
		{
			if(viewModel.SolrUnavailable) {
				labelWarning.Markup = "<span color='red'>Сервер быстрого поиска недоступен</span>";
				labelWarning.Visible = true;
			} else {
				labelWarning.Markup = "";
				labelWarning.Visible = false;
			}
		}

		void SolrSingleEntrySearchView_Shown(object sender, EventArgs e)
		{
			UpdateWarningMessage();
			searchResult.Binding.InitializeFromSource();
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
			if(viewModel.CompletionViewModel.Visible && !viewModel.SolrUnavailable) {
				ShowCompletion();
			} else {
				HideCompletion();
			}
		}

		private void ShowCompletion()
		{
			if(!Visible) {
				return;
			}

			FindWindow();

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

		void SolrSingleEntrySearchView_ParentSet(object o, ParentSetArgs args)
		{
			FindWindow();
		}

		private Gtk.Window parentGtkWindow;
		public Gtk.Window ParentGtkWindow {
			get { return parentGtkWindow; }
			set {
				if(parentGtkWindow != value && value != null) {
					parentGtkWindow = value;
					parentGtkWindow.WindowStateEvent -= Window_WindowStateEvent;
					parentGtkWindow.WindowStateEvent += Window_WindowStateEvent;
				}
			}
		}

		private void FindWindow()
		{
			//Поиск Gtk окна только при первой отрисовке
			if(ParentGtkWindow != null) {
				return;
			}
			Widget parent = Parent;
			while(!(parent is Gtk.Window) && parent != null) {
				parent = parent.Parent;
			}
			ParentGtkWindow = parent as Gtk.Window;
		}

		void Window_WindowStateEvent(object o, WindowStateEventArgs args)
		{
			//Костыль. При переходе окна в Maximized режим, при первой отрисовке, 
			//неправильно считываются позиции окна, из-за этого приходится ждать 
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
			ParentGtkWindow.GetPosition(out int winX, out int winY);
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
			viewModel.CompletionViewModel.VisibleForSearchEntry = true;
		}

		void EntrySearch_FocusOutEvent(object o, Gtk.FocusOutEventArgs args)
		{
			viewModel.CompletionViewModel.VisibleForSearchEntry = false;
		}

		protected void EntrySearch_WidgetEvent(object o, Gtk.WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.ButtonPress) {
				Gdk.EventButton buttonEvent = args.Event as Gdk.EventButton;
				if(buttonEvent.Button == 1) {
					viewModel.CompletionViewModel.VisibleOnce = true;
				}
			}

			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					viewModel.CompletionViewModel.ChooseAllResults();
				}
				if(eventKey != null && eventKey.Key == Gdk.Key.Escape) {
					HideCompletion();
				}
				if(eventKey.Key == Gdk.Key.space && eventKey.State.HasFlag(ModifierType.ControlMask)) {
					ShowCompletion();
				}
			}
		}

		protected override void OnDestroyed()
		{
			SearchCompletionView.Destroy();

			viewModel.CompletionViewModel.PropertyChanged -= CompletionViewModel_PropertyChanged;
			ClearBindings();

			base.OnDestroyed();
		}
	}
}
