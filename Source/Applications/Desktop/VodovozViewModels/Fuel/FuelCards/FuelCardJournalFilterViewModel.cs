using QS.Project.Filter;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardJournalFilterViewModel : FilterViewModelBase<FuelCardJournalFilterViewModel>
	{
		private bool _isExcludeArchived;

		public bool IsExcludeArchived
		{
			get => _isExcludeArchived;
			set => UpdateFilterField(ref _isExcludeArchived, value);
		}
	}
}
