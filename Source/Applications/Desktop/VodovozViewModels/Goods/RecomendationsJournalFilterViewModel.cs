using QS.Project.Filter;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.ViewModels.Goods
{
	public class RecomendationsJournalFilterViewModel : FilterViewModelBase<RecomendationsJournalFilterViewModel>
	{
		private PersonType? _personType;
		private RoomType? _roomType;

		public PersonType? PersonType
		{
			get => _personType;
			set => UpdateFilterField(ref _personType, value);
		}

		public RoomType? RoomType
		{
			get => _roomType;
			set => UpdateFilterField(ref _roomType, value);
		}
	}
}
