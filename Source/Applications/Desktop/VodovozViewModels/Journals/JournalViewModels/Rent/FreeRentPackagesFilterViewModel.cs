using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
	public class FreeRentPackagesFilterViewModel
		: FilterViewModelBase<FreeRentPackagesFilterViewModel>
	{
		private bool _showArchieved;

		public bool ShowArchieved
		{
			get => _showArchieved;
			set => UpdateFilterField(ref _showArchieved, value);
		}
	}
}
