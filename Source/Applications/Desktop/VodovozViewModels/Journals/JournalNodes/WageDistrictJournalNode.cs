using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Journals.JournalNodes
{
	public class WageDistrictJournalNode : JournalEntityNodeBase<WageDistrict>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public string IsArchiveString => IsArchive ? "Да" : "Нет";
		public string Name { get; set; }
		public bool IsArchive { get; set; }
	}
}
