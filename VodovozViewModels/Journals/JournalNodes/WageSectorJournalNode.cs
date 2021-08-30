using QS.Project.Journal;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Journals.JournalNodes
{
	public class WageSectorJournalNode : JournalEntityNodeBase<WageSector>
	{
		public string IsArchiveString => IsArchive ? "Да" : "Нет";
		public string RowColor => IsArchive ? "grey" : "black";

		public string Name { get; set; }
		public bool IsArchive { get; set; }
	}
}