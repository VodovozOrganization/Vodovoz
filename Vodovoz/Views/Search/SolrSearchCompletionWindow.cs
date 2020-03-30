using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding.Core;
using Gamma.ColumnConfig;
using Gtk;
using QS.DomainModel.Entity;
using SolrSearch;
using Vodovoz.SearchViewModels;
using Gamma.GtkWidgets;
using System.Collections.Specialized;

namespace Vodovoz.Views.Search
{
	public partial class SolrSearchCompletionWindow : Window
	{
		public BindingControler<SolrSearchCompletionWindow> Binding { get; private set; }

		private readonly SolrSearchCompletionViewModel completionViewModel;
		List<string> list = new List<string>();

		public SolrSearchCompletionWindow(SolrSearchCompletionViewModel completionViewModel) :
				base(WindowType.Popup)
		{
			this.Build();
			Binding = new BindingControler<SolrSearchCompletionWindow>(this);

			this.completionViewModel = completionViewModel ?? throw new ArgumentNullException(nameof(completionViewModel));
			ConfigureView();
			CreateBindings();
		}

		private void ConfigureView()
		{
			FocusInEvent += SolrSearchCompletionWindow_FocusInEvent;
			FocusOutEvent += SolrSearchCompletionWindow_FocusOutEvent;

			ytreeviewResults.Selection.Mode = SelectionMode.Single;
			ytreeviewResults.WidgetEventAfter += YtreeviewResults_WidgetEventAfter;

			ytreeviewResults.ColumnsConfig = FluentColumnsConfig<SolrSearchResult>.Create()
				.AddColumn("Тип").AddTextRenderer(x => completionViewModel.GetEntityName(x.Entity.GetType()))
					.AddSetter((cell, node) => {

						cell.BackgroundGdk = new Gdk.Color(240, 240, 245);
					})
				.AddColumn("Номер").AddTextRenderer(x => GetId(x), useMarkup: true)
					.AddSetter((cell, node) => cell.BackgroundGdk = new Gdk.Color(240, 240, 240))
				.AddColumn("Название").AddTextRenderer(x => GetTitle(x), useMarkup: true)
				.AddColumn("")
				.Finish();
			ytreeviewResults.HeadersVisible = false;
			buttonSelectAll.Clicked += ButtonSelectAll_Clicked;
			completionViewModel.SearchEntityTypes.CollectionChanged += SearchEntityTypes_CollectionChanged;
			CreateEntityButtons();
		}

		private void CreateEntityButtons()
		{
			if(boxButtons != null) {
				boxButtons.Destroy();
			}
			boxButtons = new VBox();

			foreach(var entityTypeNode in completionViewModel.SearchEntityTypes) {
				yToggleButton button = new yToggleButton();
				button.Label = completionViewModel.GetEntityName(entityTypeNode.SearchType);
				button.Active = entityTypeNode.Selected;
				button.Clicked += (sender, e) => {
					entityTypeNode.Selected = button.Active;
					completionViewModel.RunSolrSearch();
				};
				boxButtons.Add(button);
				boxButtons.SetChildPacking(button, false, false, 0, PackType.Start);
			}
			boxButtons.ShowAll();
			vboxEntityButtons.Add(boxButtons);
			vboxEntityButtons.ShowAll();
		}

		private void CreateBindings()
		{
			Binding.AddBinding(completionViewModel, vm => vm.Visible, w => w.Visible).InitializeFromSource();
			labelSummaryFound.Binding.AddFuncBinding(completionViewModel, vm => BoldLabel(vm.FoundCount.ToString()), w => w.LabelProp).InitializeFromSource();
			labelSummaryLoaded.Binding.AddFuncBinding(completionViewModel, vm => BoldLabel(vm.LoadCount.ToString()), w => w.LabelProp).InitializeFromSource();
			ytreeviewResults.Binding.AddBinding(completionViewModel, vm => vm.SearchResults, w => w.ItemsDataSource).InitializeFromSource();
		}

		/// <summary>
		/// Необходимо для удаления ссылки на Source у биндинга, 
		/// в случае если Source оставался тем же объектом, 
		/// а биндинги несколько раз пересоздавались, 
		/// например при пересоздании View для той же самой ViewModel
		/// </summary>
		private void ClearBindings()
		{
			Binding.CleanSources();
			labelSummaryFound.Binding.CleanSources();
			labelSummaryLoaded.Binding.CleanSources();
			ytreeviewResults.Binding.CleanSources();
		}

		VBox boxButtons;

		void SearchEntityTypes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CreateEntityButtons();
		}

		void SolrSearchCompletionWindow_FocusInEvent(object o, FocusInEventArgs args)
		{
			completionViewModel.VisibleWhileFocused = true;
		}

		private string BoldLabel(string inputString)
		{
			return $"<b>{inputString}</b>";
		}

		void SolrSearchCompletionWindow_FocusOutEvent(object o, FocusOutEventArgs args)
		{
			completionViewModel.VisibleWhileFocused = false;
		}

		private string GetTitle(SolrSearchResult solrSearchResult)
		{
			return solrSearchResult.Entity.GetTitle(solrSearchResult.Highlights);
		}

		private string GetId(SolrSearchResult solrSearchResult)
		{
			IDomainObject entity = solrSearchResult.Entity as IDomainObject;
			if(entity == null) {
				return "";
			}

			if(!solrSearchResult.Highlights.TryGetValue(nameof(entity.Id), out string id)) {
				id = entity.Id.ToString();
			}
			return id;
		}

		protected void OnYtreeviewResultsSelectCursorRow(object o, SelectCursorRowArgs args)
		{
			var sdsd = args;
		}

		void ButtonSelectAll_Clicked(object sender, EventArgs e)
		{
			completionViewModel.ChooseAllResults();
		}

		void YtreeviewResults_WidgetEventAfter(object o, WidgetEventAfterArgs args)
		{
			if(args.Event.Type == Gdk.EventType.KeyPress) {
				Gdk.EventKey eventKey = args.Args.OfType<Gdk.EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					SelectResult();
				}
			}

			if(args.Event.Type == Gdk.EventType.ButtonPress && (args.Event as Gdk.EventButton).Button == 1) {
				SelectResult();
			}
		}

		private void SelectResult()
		{
			SolrSearchResult selectedResult = ytreeviewResults.GetSelectedObject() as SolrSearchResult;
			if(selectedResult == null) {
				return;
			}
			completionViewModel.ChooseOneResult(selectedResult);
		}

		public void SelectFirst()
		{
			var firstResult = completionViewModel.SearchResults.FirstOrDefault();
			if(firstResult == null) {
				return;
			}
			ytreeviewResults.SelectObject(firstResult);
		}

		protected override void OnDestroyed()
		{
			ClearBindings();
			base.OnDestroyed();
		}
	}
}
