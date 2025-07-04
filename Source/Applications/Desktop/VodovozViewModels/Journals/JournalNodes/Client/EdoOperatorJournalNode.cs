using QS.Project.Journal;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class EdoOpeartorJournalNode : JournalEntityNodeBase<EdoOperator>
	{
		public string Name { get; set; }
		public string BrandName { get; set; }
		public string Code { get; set; }
		public override string Title => Name;
	}
}
