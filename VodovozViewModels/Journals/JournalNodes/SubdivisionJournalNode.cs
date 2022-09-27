using QS.Project.Journal;
using System.Collections.Generic;

namespace Vodovoz.Journals.JournalNodes
{
	public class SubdivisionJournalNode : JournalEntityNodeBase
	{
		protected SubdivisionJournalNode() : base(typeof(Subdivision))
		{
		}

		public override string Title => Name;

		public string Name { get; set; }

		public string ChiefName { get; set; }
		public virtual SubdivisionJournalNode Parent { get; set; }
		public virtual int? ParentId { get; set; }
		public virtual IList<SubdivisionJournalNode> Children { get; set; }
	}
}
