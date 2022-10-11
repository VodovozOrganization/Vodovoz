using System;
using QS.Project.Journal;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Complaints
{
	public class ComplaintObjectJournalNode : JournalEntityNodeBase<ComplaintObject>
	{
		public override string Title => Name;
		public DateTime CreateDate { get; set; }
		public string Name { get; set; }
		public string ComplaintKinds { get; set; }
		public bool IsArchive { get; set; }
		public DateTime ArchiveDate { get; set; }
	}
}
