using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System.Collections.Generic;

namespace Vodovoz.Journals.JournalNodes
{
	public class SubdivisionJournalNode : JournalNodeBase,
		IHierarchicalNode<SubdivisionJournalNode>
	{
		public override string Title => Name;

		public string Name { get; set; }
		public int Id { get; set; }
		public string ChiefName { get; set; }
		public virtual SubdivisionJournalNode Parent { get; set; }
		public virtual int? ParentId { get; set; }
		public IList<SubdivisionJournalNode> Children { get; set; } = new List<SubdivisionJournalNode>();
		public bool IsArchive { get; set; }
	}
}
