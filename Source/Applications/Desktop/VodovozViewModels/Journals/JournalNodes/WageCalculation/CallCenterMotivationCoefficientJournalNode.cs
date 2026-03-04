using System;
using System.Collections.Generic;
using QS.Project.Journal.DataLoader.Hierarchy;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.WageCalculation
{
	public class CallCenterMotivationCoefficientJournalNode : IHierarchicalNode<CallCenterMotivationCoefficientJournalNode>
	{
		public string Title => Name;
		public int Id { get; set; }
		public string Name { get; set; }
		public int? ParentId { get; set; }
		public bool IsArchive { get; set; }
		public Type JournalNodeType { get; set; }
		public CallCenterMotivationCoefficientJournalNode Parent { get; set; }
		public IList<CallCenterMotivationCoefficientJournalNode> Children { get; set; } = new List<CallCenterMotivationCoefficientJournalNode>();
		public NomenclatureMotivationUnitType? MotivationUnitType { get; set; }
		public string MotivationCoefficientText { get; set; }
		public decimal? MotivationCoefficient => decimal.TryParse(MotivationCoefficientText, out var coefficient) ? coefficient : (decimal?)null;
	}
}
