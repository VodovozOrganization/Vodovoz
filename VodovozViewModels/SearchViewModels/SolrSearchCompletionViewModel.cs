using System;
using System.Collections.Generic;
using QS.ViewModels;
using SolrSearch;
using Vodovoz.SearchModel;
using QS.DomainModel.Entity;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Vodovoz.SearchViewModels
{
	public class SolrSearchCompletionViewModel : ViewModelBase
	{
		private bool visibleForSearchEntry;
		/// <summary>
		/// Показывает completion если фокус находится в строке ввода
		/// </summary>
		/// <value><c>true</c> if visible for search entry; otherwise, <c>false</c>.</value>
		public virtual bool VisibleForSearchEntry {
			get => visibleForSearchEntry;
			set {
				if(SetField(ref visibleForSearchEntry, value)) {
					OnPropertyChanged(nameof(Visible));
				}
			}
		}

		private bool visibleWhileFocused;
		/// <summary>
		/// Показывает completion если фокус ввода на окне completion
		/// </summary>
		/// <value><c>true</c> if visible while focused; otherwise, <c>false</c>.</value>
		public virtual bool VisibleWhileFocused {
			get => visibleWhileFocused;
			set {
				if(SetField(ref visibleWhileFocused, value)) {
					OnPropertyChanged(nameof(Visible));
				}
			}
		}

		private bool visibleOnce;
		/// <summary>
		/// Показывает completion при получении новых результатов поиска
		/// </summary>
		public virtual bool VisibleOnce {
			get => visibleOnce;
			set {
				if(SetField(ref visibleOnce, value, () => VisibleOnce)) {
					OnPropertyChanged(nameof(Visible));
				}
			}
		}

		public bool HasSearchValue => SearchModel.SearchValues != null && SearchModel.SearchValues.Any();

		public bool Visible => visibleOnce && (VisibleForSearchEntry || VisibleWhileFocused) && HasSearchValue;

		private int foundCount;
		public virtual int FoundCount {
			get => foundCount;
			private set => SetField(ref foundCount, value, () => FoundCount);
		}

		private int loadCount;
		public virtual int LoadCount {
			get => loadCount;
			private set => SetField(ref loadCount, value, () => LoadCount);
		}

		IEnumerable<SolrSearchResult> searchResults = new List<SolrSearchResult>();
		public virtual IEnumerable<SolrSearchResult> SearchResults {
			get => searchResults;
			set {
				if(SetField(ref searchResults, value)) {
					VisibleOnce = true;
					visibleOnce = false;
				}
			}
		}

		public ObservableCollection<SolrSearchTypeSelectableNode> SearchEntityTypes = new ObservableCollection<SolrSearchTypeSelectableNode>();

		public SolrCriterionSearchModel SearchModel { get; private set; }

		public SolrSearchCompletionViewModel(SolrCriterionSearchModel searchModel)
		{
			SearchModel = searchModel ?? throw new ArgumentNullException(nameof(searchModel));
			SearchModel.SearchEntityTypes.CollectionChanged += SearchEntityTypes_CollectionChanged;
		}

		void SearchEntityTypes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach(Type type in e.NewItems) {
						if(SearchEntityTypes.Any(x => x.SearchType == type)) {
							continue;
						}
						SearchEntityTypes.Add(new SolrSearchTypeSelectableNode(type));
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach(Type type in e.OldItems as IEnumerable<Type>) {
						var removedItem = SearchEntityTypes.FirstOrDefault(x => x.SearchType == type);
						if(removedItem != null) {
							SearchEntityTypes.Remove(removedItem);
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach(Type type in e.OldItems as IEnumerable<Type>) {
						var removedItem = SearchEntityTypes.FirstOrDefault(x => x.SearchType == type);
						if(removedItem != null) {
							SearchEntityTypes.Remove(removedItem);
						}
					}
					foreach(Type type in e.NewItems as IEnumerable<Type>) {
						SearchEntityTypes.Add(new SolrSearchTypeSelectableNode(type));
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					SearchEntityTypes.Clear();
					foreach(var type in SearchModel.SearchEntityTypes) {
						SearchEntityTypes.Add(new SolrSearchTypeSelectableNode(type));
					}
					break;
				default:
					break;
			}
		}

		public void RunSolrSearch()
		{
			var solrSearchResults = SearchModel.RunSolrSearch(SearchEntityTypes.Where(x => x.Selected).Select(x => x.SearchType));
			if(solrSearchResults == null) {
				FoundCount = 0;
				LoadCount = 0;
				SearchResults = new SolrSearchResult[] { };
				return;
			}

			FoundCount = solrSearchResults.FoundCount;
			LoadCount = solrSearchResults.LoadCount;
			SearchResults = solrSearchResults.Results;
		}

		public void ChooseOneResult(SolrSearchResult selectedResult)
		{
			OnPropertyChanged(nameof(Visible));
			SearchModel.SelectedResults = new SolrSearchResult[] { selectedResult };
		}

		public void ChooseAllResults()
		{
			OnPropertyChanged(nameof(Visible));
			SearchModel.SelectedResults = SearchResults;
		}

		public string GetEntityName(Type type)
		{
			if(type == null) {
				throw new ArgumentNullException(nameof(type));
			}

			return DomainHelper.GetSubjectName(type);
		}
	}

	public class SolrSearchTypeSelectableNode
	{
		public virtual bool Selected { get; set; } = true;

		public Type SearchType { get; }

		public SolrSearchTypeSelectableNode(Type searchType)
		{
			SearchType = searchType ?? throw new ArgumentNullException(nameof(searchType));
		}
	}
}
