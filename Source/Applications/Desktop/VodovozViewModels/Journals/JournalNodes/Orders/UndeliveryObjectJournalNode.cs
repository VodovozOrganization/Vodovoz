using System;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class UndeliveryObjectJournalNode : JournalEntityNodeBase<UndeliveryObject>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string UndeliveryKinds { get; set; }
		public bool IsArchive { get; set; }
		public DateTime ArchiveDate { get; set; }
	}
}
