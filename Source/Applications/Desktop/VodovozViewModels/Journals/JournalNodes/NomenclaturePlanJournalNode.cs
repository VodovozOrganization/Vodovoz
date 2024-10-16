using QS.Project.Journal;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class NomenclaturePlanJournalNode : JournalEntityNodeBase<Nomenclature>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public NomenclatureCategory Category { get; set; }
		public string OnlineStoreExternalId { get; set; }
		public int PlanDay { get; set; }
		public int PlanMonth { get; set; }
    }
}
