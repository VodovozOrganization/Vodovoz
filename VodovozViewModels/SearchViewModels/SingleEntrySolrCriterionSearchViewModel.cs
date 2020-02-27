using QS.Project.Journal.Search;
using Vodovoz.SearchModel;
namespace Vodovoz.SearchViewModels
{
	public class SingleEntrySolrCriterionSearchViewModel : SingleEntrySearchViewModel<SolrCriterionSearchModel>
	{

		#region Результаты поиска

		public virtual bool SearchResultVisible => !string.IsNullOrWhiteSpace(SearchResult);

		private string searchResult;
		public virtual string SearchResult {
			get => searchResult;
			set {
				if(SetField(ref searchResult, value)) {
					OnPropertyChanged(nameof(SearchResultVisible));
				}
			}
		}

		#endregion Результаты поиска

		#region Поисковые строки

		private string searchValue;
		public override string SearchValue {
			get => searchValue;
			set {
				if(SetField(ref searchValue, value, () => SearchValue)) {
					OnSearchValueUpdated();
				}
			}
		}

		#endregion Поисковые строки

		private SolrSearchCompletionViewModel completionViewModel;
		public virtual SolrSearchCompletionViewModel CompletionViewModel {
			get => completionViewModel;
			set => SetField(ref completionViewModel, value, () => CompletionViewModel);
		}

		public SingleEntrySolrCriterionSearchViewModel(SolrCriterionSearchModel searchModel) : base(searchModel)
		{
			CompletionViewModel = new SolrSearchCompletionViewModel(SearchModelGeneric);
			SearchModelGeneric.PropertyChanged += SearchModelGeneric_PropertyChanged;
		}

		void SearchModelGeneric_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(SearchModelGeneric.SelectedResults):
					CompletionViewModel.VisibleForSearchEntry = true;
					break;
				default:
					break;
			}
		}

		/*
		public override void ClearSearchValues()
		{
			//base.ClearSearchValues();
			SearchResult1 = string.Empty;
			SearchResult2 = string.Empty;
			SearchResult3 = string.Empty;
			SearchResult4 = string.Empty;
		}*/

		public override void UpdateSearchModel(string[] values)
		{
			// метод обновления поисковых строк в модели поиска
			// переопределен для блокировки вызова у модели автоматического 
			// обновления модели, чтобы модель не уведомляла что поиск закончен, 
			// так как выходные данные еще не выбраны
			SearchModelGeneric.SearchValues = values;
			var solrResult = SearchModelGeneric.RunSolrSearch();
			CompletionViewModel.SearchResults = solrResult;

		}

		public override void ManualSearchUpdate()
		{
			// при ручном вызове поиска

			base.ManualSearchUpdate();

			// запускаем обновление модели, говорит о том что поиск полностью завершен и данные выбраны 
			SearchModelGeneric.Update();
		}

		protected override void OnSearchValueUpdated()
		{
			// при изменении строки поиска
			UpdateSearchModel(new string[] { SearchValue });
		}
	}
}
