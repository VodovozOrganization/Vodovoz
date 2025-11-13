using QS.Project.Filter;

namespace Vodovoz.Filters.ViewModels
{
	public class ProfitCategoriesJournalFilterViewModel : FilterViewModelBase<ProfitCategoriesJournalFilterViewModel>
	{
		private bool _showArchive;

		public bool ShowArchive
		{
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value);
		}
	}
}
