using System;
using QS.Project.Journal.Search.Criterion;
using Vodovoz.SearchViewModels;
namespace Vodovoz.Core
{
	public static class CriterionSearchFactory
	{
		public static ICriterionSearch GetMultipleEntryCriterionSearch()
		{
			return new MultipleEntryCriterionSearch(new DelayedMultipleEntryCriterionSearchViewModel(new CriterionSearchModel()));
		}

		public static ICriterionSearch GetSingleEntryCriterionSearch()
		{
			return new SingleEntryCriterionSearch(new DelayedSingleEntryCriterionSearchViewModel(new CriterionSearchModel()));
		}
	}
}
