using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesJournalNode : JournalNodeBase, IHierarchicalNode<FinancialCategoriesJournalNode>
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public Type JournalNodeType { get; set; }
		public FinancialCategoriesJournalNode Parent { get; set; }
		public IList<FinancialCategoriesJournalNode> Children { get; set; } = new List<FinancialCategoriesJournalNode>();
	}
}
