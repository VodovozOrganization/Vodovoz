using System;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Journals.JournalNodes
{
	public class SectorJournalNode : JournalEntityNodeBase<Sector>
	{
		public override string Title => Id.ToString();
		public string Name { get; set; }
		public string WageDistrict { get; set; }
		public SectorsSetStatus SectorsSetStatus { get; set; }
		public int SectorVersionId { get; set; }
	}
}
