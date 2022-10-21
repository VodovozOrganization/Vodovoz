using System;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Journals.JournalNodes
{
	public class DistrictJournalNode : JournalEntityNodeBase<District>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string WageDistrict { get; set; }
		public DistrictsSetStatus DistrictsSetStatus { get; set; }
		public int DistrictsSetId { get; set; }
	}
}
