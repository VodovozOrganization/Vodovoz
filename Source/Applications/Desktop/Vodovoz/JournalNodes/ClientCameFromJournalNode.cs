using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.JournalNodes
{
	public class ClientCameFromJournalNode : JournalEntityNodeBase<ClientCameFrom>
	{
		public override string Title => Name;
		public bool IsArchive { get; set; }
		public string Name { get; set; }
	}
}
