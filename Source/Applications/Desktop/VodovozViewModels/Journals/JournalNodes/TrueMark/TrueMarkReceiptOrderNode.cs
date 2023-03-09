using Gamma.Utilities;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class TrueMarkReceiptOrderNode : JournalNodeBase, IHierarchicalNode<TrueMarkReceiptOrderNode>
	{
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public TrueMarkReceiptOrderNode Parent { get; set; }
		public IList<TrueMarkReceiptOrderNode> Children { get; set; }

		public TrueMarkOrderNodeType NodeType { get; set; }
		public string EntityId => Id == 0 ? "" : Id.ToString();
		public override string Title => $"{EntityId} {Time:dd.MM.yyyy HH:mm:ss}";


		public DateTime? Time { get; set; }
		public int OrderAndItemId { get; set; }
		public int RouteListId { get; set; }
		public string DriverName { get; set; }
		public string DriverLastName { get; set; }
		public string DriverPatronimyc { get; set; }
		public string DriverFIO => PersonHelper.PersonNameWithInitials(DriverLastName, DriverName, DriverPatronimyc);
		public TrueMarkCashReceiptOrderStatus? OrderStatus { get; set; }
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
