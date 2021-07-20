using System;
using QS.Project.Journal;
using Vodovoz.Domain;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Flyers
{
	public class FlyersJournalNode : JournalEntityNodeBase<Flyer>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}