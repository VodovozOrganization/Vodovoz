using QS.Project.Journal;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class GeoGroupJournalNode : JournalEntityNodeBase<GeoGroup>
	{
		public string Name { get; set; }

		public string Title => Name;
	}
}
