using System;
using System.Collections.Generic;
using Gamma.Binding.Core;
using Gamma.ColumnConfig;
using Gtk;
using QS.DomainModel.Entity;
using SolrSearch;
using Vodovoz.SearchViewModels;

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

			FocusInEvent += SolrSearchCompletionWindow_FocusInEvent;
			FocusOutEvent += SolrSearchCompletionWindow_FocusOutEvent;

			Binding.AddBinding(completionViewModel, vm => vm.Visible, w => w.Visible).InitializeFromSource();

			ytreeviewResults.Selection.Mode = SelectionMode.Single;
			ytreeviewResults.Selection.Changed += Selection_Changed;
			ytreeviewResults.ColumnsConfig = FluentColumnsConfig<SolrSearchResult>.Create()
				.AddColumn("Тип").AddTextRenderer(x => DomainHelper.GetSubjectName(x.Entity.GetType()))
				.AddColumn("Номер").AddTextRenderer(x => GetTitle(x), useMarkup: true)
				.AddColumn("Название").AddTextRenderer(x => GetTitle(x), useMarkup: true)
				.AddColumn("")
				.Finish();
			ytreeviewResults.HeadersVisible = false;
			ytreeviewResults.Binding.AddBinding(completionViewModel.SearchModel, vm => vm.SelectedResults, w => w.ItemsDataSource).InitializeFromSource();

			buttonSelectAll.Clicked += ButtonSelectAll_Clicked;
		}

		void SolrSearchCompletionWindow_FocusInEvent(object o, FocusInEventArgs args)
		{
			completionViewModel.VisibleWhileFocused = true;
		}


		void SolrSearchCompletionWindow_FocusOutEvent(object o, FocusOutEventArgs args)
		{
			completionViewModel.VisibleWhileFocused = false;
		}


		void Selection_Changed(object sender, EventArgs e)
		{
			//completionViewModel.SearchResults ytreeviewResults.GetSelectedObject();
		}

		private string GetTitle(SolrSearchResult solrSearchResult)
		{
			return solrSearchResult.Entity.GetTitle(solrSearchResult.Highlights);
		}

		private string GetId(SolrSearchResult solrSearchResult)
		{
			if(solrSearchResult.Entity is IDomainObject) {
				return (solrSearchResult.Entity as IDomainObject).Id.ToString();
			}
			return "";
		}

		protected void OnYtreeviewResultsSelectCursorRow(object o, SelectCursorRowArgs args)
		{
			var sdsd = args;
		}

		void ButtonSelectAll_Clicked(object sender, EventArgs e)
		{
			Console.WriteLine("Select All");
		}

	}
}
