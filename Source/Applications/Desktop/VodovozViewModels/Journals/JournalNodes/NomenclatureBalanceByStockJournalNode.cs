using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class NomenclatureBalanceByStockJournalNode : JournalEntityNodeBase<Warehouse>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public string WarehouseName { get; set; }
		public decimal NomenclatureAmount { get; set; }
	}
}
