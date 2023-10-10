using QS.Project.Journal;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Journals.JournalNodes
{
	public class SalesPlanJournalNode : JournalEntityNodeBase<SalesPlan>
	{
		public string IsArchiveString => IsArchive ? "Да" : "Нет";
		public override string Title {
			get {
				return string.Format(
					"продажа - {0} бут., забор - {1} бут.",
					FullBottleToSell,
					EmptyBottlesToTake
				);
			}
		}

		public string Name { get; set; }
		public int FullBottleToSell { get; set; }
		public int EmptyBottlesToTake { get; set; }
		public bool IsArchive { get; set; }
	}
}
