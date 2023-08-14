using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesJournalNode : JournalNodeBase, IHierarchicalNode<FinancialCategoriesJournalNode>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public string Numbering { get; set; }
		public Type JournalNodeType { get; set; }
		public FinancialSubType FinancialSubType { get; set; }
		public FinancialCategoriesJournalNode Parent { get; set; }
		public IList<FinancialCategoriesJournalNode> Children { get; set; } = new List<FinancialCategoriesJournalNode>();
	}
}
