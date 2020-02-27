using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QS.ViewModels;
using SolrSearch;
using Vodovoz.SearchModel;

namespace Vodovoz.SearchViewModels
{
	public class SolrSearchCompletionViewModel : ViewModelBase
	{
		private bool visibleForSearchEntry;
		public virtual bool VisibleForSearchEntry {
			get => visibleForSearchEntry;
			set {
				if(SetField(ref visibleForSearchEntry, value)) {
					OnPropertyChanged(nameof(Visible));
				}
			}
		}

		private bool visibleWhileFocused;
		public virtual bool VisibleWhileFocused {
			get => visibleWhileFocused;
			set {
				if(SetField(ref visibleWhileFocused, value)) {
					OnPropertyChanged(nameof(Visible));
				}
			}
		}

		public bool Visible => VisibleForSearchEntry || VisibleWhileFocused;

		IEnumerable<SolrSearchResult> searchResults = new List<SolrSearchResult>();
		public virtual IEnumerable<SolrSearchResult> SearchResults {
			get => searchResults;
			set => SetField(ref searchResults, value);
		}

		public SolrCriterionSearchModel SearchModel { get; private set; }

		public SolrSearchCompletionViewModel(SolrCriterionSearchModel searchModel)
		{
			SearchModel = searchModel ?? throw new ArgumentNullException(nameof(searchModel));
		}

		public void SelectOneResult(SolrSearchResult selectedResult)
		{
			SearchModel.SelectedResults = SearchResults;
		}
	}
}
