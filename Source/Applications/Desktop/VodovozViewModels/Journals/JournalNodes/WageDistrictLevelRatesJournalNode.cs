using QS.Project.Journal;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Journals.JournalNodes
{
	public class WageDistrictLevelRatesJournalNode : JournalEntityNodeBase<WageDistrictLevelRates>
	{
		public override string Title => Name;
		public string IsArchiveString => IsArchive ? "Да" : "Нет";
		public string IsDefaultLevelString => IsDefaultLevel ? "Да" : string.Empty;
		public string IsDefaultLevelOurCarsString => IsDefaultLevelOurCars ? "Да" : string.Empty;
		public string IsDefaultLevelRaskatCarsString => IsDefaultLevelRaskatCars ? "Да" : string.Empty;
		public string RowColor => IsArchive ? "grey" : "black";

		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public bool IsDefaultLevel { get; set; }
		public bool IsDefaultLevelOurCars{ get; set; }
		public bool IsDefaultLevelRaskatCars { get; set; }
	}
}
