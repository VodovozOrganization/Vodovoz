using Gamma.Utilities;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class CashReceiptJournalNode : JournalNodeBase, IHierarchicalNode<CashReceiptJournalNode>
	{
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public CashReceiptJournalNode Parent { get; set; }
		public IList<CashReceiptJournalNode> Children { get; set; }

		public CashReceiptNodeType NodeType { get; set; }
		public string EntityId => Id == 0 ? "" : Id.ToString();
		public override string Title => $"{EntityId} {Time:dd.MM.yyyy HH:mm:ss}";


		public DateTime? Time { get; set; }
		public int OrderAndItemId { get; set; }
		public CashReceiptStatus? OrderStatus { get; set; }
		public string Status => OrderStatus.HasValue ? OrderStatus.GetEnumTitle() : "";
		public string UnscannedReason { get; set; }
		public string ErrorDescription { get; set; }

		public bool IsDefectiveCode { get; set; }
		public bool IsDuplicateCode { get; set; }

		public string SourceGtin { get; set; }
		public string SourceSerialnumber { get; set; }
		public string ResultGtin { get; set; }
		public string ResultSerialnumber { get; set; }

		public int? ReceiptId { get; set; }
		public bool HasReceipt => ReceiptId.HasValue;


	}
}
