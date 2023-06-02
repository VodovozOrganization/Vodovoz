using System;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
	public class SelfDeliveryJournalNode : JournalEntityNodeBase<Order>
	{
		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public decimal BottleAmount { get; set; }

		public string Counterparty { get; set; }

		public PaymentType PaymentTypeEnum { get; set; }
		public bool PayAfterLoad { get; set; }
		public string PayOption => PayAfterLoad ? "После погрузки" : "До погрузки";

		//заказ
		public decimal OrderSum { get; set; }
		public decimal OrderReturnSum { get; set; }
		public decimal OrderCashSumTotal => (PaymentTypeEnum == 
				PaymentType.Cash || (PaymentTypeEnum == PaymentType.Terminal && StatusEnum == OrderStatus.WaitForPayment) || (PaymentTypeEnum == PaymentType.Terminal && PayAfterLoad && StatusEnum != OrderStatus.Closed)) 
				? OrderSum - OrderReturnSum 
				: 0;
		public decimal OrderCashlessSumTotal => PaymentTypeEnum == PaymentType.Cashless || 
		                                        PaymentTypeEnum == PaymentType.PaidOnline ||
		                                        PaymentTypeEnum == PaymentType.Terminal ? OrderReturnSum - OrderReturnSum : 0;

		//наличные по кассе
		public decimal CashPaid { get; set; }
		public decimal CashReturn { get; set; }
		public decimal CashTotal => CashPaid - CashReturn;

		public decimal TotalCashDiff => OrderCashSumTotal - CashTotal;

		public bool HasCashDiff => OrderCashSumTotal != CashTotal;

		public string AuthorLastName { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);

		public string RowColor {
			get {
				if(CashPaid > 0 && HasCashDiff)
					return "#f97777";//light red
				if(StatusEnum == OrderStatus.Closed && HasCashDiff)
					return "#ee0000";//red
				return "black";
			}
		}
	}
}
