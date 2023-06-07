using QS.Project.Journal;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class ProductGroupJournalNode: JournalEntityNodeBase<ProductGroup>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public ProductGroupJournalNode Parent { get; set; }
		public int? ParentId { get; set; }

	}
}
