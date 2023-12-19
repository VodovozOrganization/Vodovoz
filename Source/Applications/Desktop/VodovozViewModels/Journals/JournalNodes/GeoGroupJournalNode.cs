using QS.Project.Journal;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class GeoGroupJournalNode : JournalEntityNodeBase<GeoGroup>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchived { get; set; }
	}
}
