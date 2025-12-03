using QS.Project.Journal;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.ViewModels.Goods
{
	public class RecomendationJournalNode : JournalNodeBase
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public bool IsArchive { get; internal set; }
		public PersonType? PersonType { get; internal set; }
		public RoomType? RoomType { get; internal set; }
		public override string Title => Name;
	}
}
