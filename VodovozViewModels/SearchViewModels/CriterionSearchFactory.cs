using QS.Project.Journal.Search.Criterion;
using QS.Project.Journal.Search;

namespace Vodovoz.SearchViewModels
{
	public static class CriterionSearchFactory
	{
		public static MultipleEntrySearchViewModel<CriterionSearchModel> GetMultipleEntryCriterionSearchViewModel()
		{
			return new DelayedMultipleEntryCriterionSearchViewModel<CriterionSearchModel>(new CriterionSearchModel());
		}

		public static SingleEntrySearchViewModel<CriterionSearchModel> GetSingleEntryCriterionSearchViewModel()
		{
			return new DelayedSingleEntryCriterionSearchViewModel<CriterionSearchModel>(new CriterionSearchModel());
		}
	}
}
