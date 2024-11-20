using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Goods.ProductGroups
{
	public class ProductGroupsJournalNode : JournalNodeBase, IHierarchicalNode<ProductGroupsJournalNode>
	{
		public override string Title => Name;
		public int Id { get; set; }
		public string Name { get; set; }
		public int? ParentId { get; set; }
		public bool IsArchive { get; set; }
		public Type JournalNodeType { get; set; }
		public ProductGroupsJournalNode Parent { get; set; }
		public IList<ProductGroupsJournalNode> Children { get; set; } = new List<ProductGroupsJournalNode>();
	}
}
