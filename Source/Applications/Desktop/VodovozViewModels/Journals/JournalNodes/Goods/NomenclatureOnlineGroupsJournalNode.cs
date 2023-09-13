using QS.Project.Journal;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class NomenclatureOnlineGroupsJournalNode : JournalEntityNodeBase<NomenclatureOnlineGroup>
	{
		public string Name { get; set; }
		public string OnlineCategories { get; set; }
		public override string Title => Name;
	}
}
