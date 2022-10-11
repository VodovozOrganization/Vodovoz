using QS.DomainModel.Entity;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class TrackPointJournalFilterViewModel : FilterViewModelBase<TrackPointJournalFilterViewModel>
	{
		private const string _routeListName = "Маршрутный лист";

		private int? _routeListId;

		[PropertyChangedAlso(nameof(RouteListLabelText))]
		public int? RouteListId
		{
			get => _routeListId;
			set => SetField(ref _routeListId, value);
		}

		public string RouteListLabelText => 
			RouteListId == null
			? $"<span foreground = \"red\">{ _routeListName }</span>"
			: _routeListName;
	}
}
