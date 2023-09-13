using QS.Project.Journal;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class NomenclatureOnlineCategoriesJournalNode : JournalEntityNodeBase<NomenclatureOnlineCategory>
	{
		public string Name { get; set; }
		public string OnlineGroup { get; set; }
		public override string Title => Name;
	}
}
