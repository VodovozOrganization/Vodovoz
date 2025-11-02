using QS.Project.Filter;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardJournalFilterViewModel : FilterViewModelBase<FuelCardJournalFilterViewModel>
	{
		private bool _isShowArchived;

		public bool IsShowArchived
		{
			get => _isShowArchived;
			set => UpdateFilterField(ref _isShowArchived, value);
		}
	}
}
