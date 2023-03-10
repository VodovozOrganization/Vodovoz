using Gamma.Utilities;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class CashReceiptNode : JournalNodeBase
	{
		public override string Title => $"{OrderId}";

		public int OrderId { get; set; }
		public int? ReceiptId { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public DateTime? ReceiptTime { get; set; }
		public decimal OrderSum { get; set; }
		public PaymentType OrderPaymentType { get; set; }
		public string PaymentType => OrderPaymentType.GetEnumTitle();
		public bool IsSelfdelivery { get; set; }

		public int RouteListId { get; set; }
		public string DriverName { get; set; }
		public string DriverLastName { get; set; }
		public string DriverPatronimyc { get; set; }
		public string DriverFIO => PersonHelper.PersonNameWithInitials(DriverLastName, DriverName, DriverPatronimyc);
		public TrueMarkCashReceiptOrderStatus? ReceiptStatus { get; set; }
		public string Status
		{
			get
			{
				if(!ReceiptId.HasValue)
				{
					return TrueMarkCashReceiptOrderStatus.ReceiptNotNeeded.GetEnumTitle();
				}
				return ReceiptStatus.HasValue ? ReceiptStatus.GetEnumTitle() : "";
			}
		}

		public string UnscannedReason { get; set; }
		public string ErrorDescription { get; set; }
	}
}
