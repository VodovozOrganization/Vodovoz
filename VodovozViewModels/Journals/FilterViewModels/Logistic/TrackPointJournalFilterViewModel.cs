using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class TrackPointJournalFilterViewModel : FilterViewModelBase<TrackPointJournalFilterViewModel>
	{
		private int? _routeListId;

		public int? RouteListId
		{
			get => _routeListId;
			set => SetField(ref _routeListId, value);
		}
	}
}
