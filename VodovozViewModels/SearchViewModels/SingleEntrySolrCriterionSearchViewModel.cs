using System;
using System.Threading;
using System.Threading.Tasks;
using QS.Project.Journal.Search;
using Vodovoz.SearchModel;
using System.Linq;
using QS.DomainModel.Entity;
using SolrSearch;
namespace Vodovoz.SearchViewModels
{
	public class SingleEntrySolrCriterionSearchViewModel : SingleEntrySearchViewModel<SolrCriterionSearchModel>
	{
		public bool SolrUnavailable => SearchModelGeneric.SolrUnavailable;

		public virtual bool SolrDisable {
			get => SearchModelGeneric.SolrDisable;
			set => SearchModelGeneric.SolrDisable = value;
		}

		private int standartSearchStartDelay = 1000;
		/// <summary>
		/// Задержка для ввода нового значений, перед стартом стандартного поиска
		/// </summary>
		public virtual int StandartSearchStartDelay {
			get => standartSearchStartDelay;
			set => SetField(ref standartSearchStartDelay, value, () => StandartSearchStartDelay);
		}


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
					UpdateSearchResult();
					break;
				case nameof(SearchModelGeneric.SolrDisable):
					OnPropertyChanged(nameof(SolrDisable));
					if(!SolrDisable) {
						cts?.Cancel();
					}
					break;
				case nameof(SearchModelGeneric.SolrUnavailable):
					OnPropertyChanged(nameof(SolrUnavailable));
					break;
				default:
					break;
			}
		}

		private void UpdateSearchResult() 
		{
			if(!SearchModelGeneric.SelectedResults.Any()) {
				SearchResult = "";
				return;
			}
			if(SearchModelGeneric.SelectedResults.Count() > 1) {
				SearchResult = $"Все результаты для: {SearchValue}";
			} else {
				var selectedResult = SearchModelGeneric.SelectedResults.First();
				IDomainObject selectedIdObj = selectedResult.Entity as IDomainObject;
				string id = selectedIdObj != null ? selectedIdObj.Id.ToString() : "";
				string title = selectedResult.Entity.GetTitle();
				SearchResult = $"Выбран: {id}. {title}";
			}
		}

		public override void Clear()
		{
			SearchValue = string.Empty;
			SearchModelGeneric.SelectedResults = new SolrSearchResult[] { };
		}

		#region Запуск поиска

		public override void UpdateSearchModel(string[] values)
		{
			// метод обновления данных в модели поиска
			if(SolrDisable) {
				StandartUpdateSearchModel(values);
			} else {
				SolrUpdateSearchModel(values);
			}
		}

		public override void ManualSearchUpdate()
		{
			// при ручном вызове поиска
			if(SolrDisable) {
				StandartManualSearchUpdate();
			} else {
				SolrManualSearchUpdate();
			}
		}

		protected override void OnSearchValueUpdated()
		{
			// при изменении строки поиска
			if(SolrDisable) {
				StandartOnSearchValueUpdated();
			} else {
				SolrOnSearchValueUpdated();
			}
		}

		#region Solr search

		private void SolrUpdateSearchModel(string[] values)
		{
			// переопределен для блокировки вызова у модели автоматического 
			// обновления модели, чтобы модель не уведомляла что поиск закончен, 
			// так как выходные данные еще не выбраны
			SearchModelGeneric.SearchValues = values;
			CompletionViewModel.RunSolrSearch();
		}

		private void SolrManualSearchUpdate()
		{
			base.ManualSearchUpdate();
			// запускаем обновление модели, говорит о том что поиск полностью завершен и данные выбраны 
			SearchModelGeneric.Update();
		}

		private void SolrOnSearchValueUpdated()
		{
			UpdateSearchModel(SearchValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
		}

		#endregion Solr search

		#region Standart search

		private CancellationTokenSource cts;

		private void StartUpdateTask()
		{
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();

			Task delayTask = Task.Delay(StandartSearchStartDelay, cts.Token);
			delayTask.ContinueWith((task) => UpdateValue(), cts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
		}

		private void UpdateValue()
		{
			if(cts == null || cts.IsCancellationRequested) {
				return;
			}
			base.OnSearchValueUpdated();
		}

		private void StandartUpdateSearchModel(string[] values)
		{
			base.UpdateSearchModel(values);
		}

		private void StandartManualSearchUpdate()
		{
			base.OnSearchValueUpdated();
		}

		private void StandartOnSearchValueUpdated()
		{
			StartUpdateTask();
		}

		#endregion Standart search

		#endregion Запуск поиска

	}
}
