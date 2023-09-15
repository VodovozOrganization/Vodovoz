using System;
using QS.Project.Journal;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class NomenclatureOnlineCatalogsJournalNode : JournalEntityNodeBase<NomenclatureOnlineCatalog>
	{
		public override string Title => Name;

		public int Id { get; set; }
		public string Name { get; set; }
		public Guid ExternalId { get; set; }
	}
}
