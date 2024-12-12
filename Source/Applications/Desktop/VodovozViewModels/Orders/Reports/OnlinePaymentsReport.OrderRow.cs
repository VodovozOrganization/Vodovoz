using Gamma.Utilities;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public partial class OnlinePaymentsReport
	{
		public partial class OrderRow
		{
			public DateTime? OrderDeliveryDate { get; internal set; }
			public int OrderId { get; internal set; }
			public string CcounterpartyFullName { get; internal set; }
			public string Address { get; internal set; }
			public int? OnlineOrderId { get; internal set; }
			public decimal TotalSumFromBank { get; internal set; }
			public decimal OrderTotalSum { get; internal set; }
			public OrderStatus OrderStatus { get; internal set; }
			public string Author { get; internal set; }
			public string PaymentDateTimeOrError { get; internal set; }
			public PaymentType OrderPaymentType { get; internal set; }
			public bool IsFutureOrder { get; internal set; }
			public string NumberAndShop { get; internal set; }
			public int? PaymentId { get; internal set; }
			public ReportPaymentStatus ReportPaymentStatusEnum { get; set; }
			public DateTime? OrderCreateDate { get; internal set; }
			public string SumAndPaid => ReportPaymentStatusEnum == ReportPaymentStatus.Missing
				? OrderTotalSum.ToString("# ##0.00")
				: $"{TotalSumFromBank:# ##0.00} из {OrderTotalSum:# ##0.00}";

			public string OrderStatusString => OrderStatus.GetEnumTitle();

			public int OrderStatusOrderingValue
			{
				get
				{
					switch(OrderStatus)
					{
						case OrderStatus.Canceled:
							return 0;
						case OrderStatus.NewOrder:
							return 1;
						case OrderStatus.WaitForPayment:
							return 2;
						case OrderStatus.Accepted:
							return 3;
						case OrderStatus.InTravelList:
							return 4;
						case OrderStatus.OnLoading:
							return 5;
						case OrderStatus.OnTheWay:
							return 6;
						case OrderStatus.DeliveryCanceled:
							return 7;
						case OrderStatus.Shipped:
							return 8;
						case OrderStatus.UnloadingOnStock:
							return 9;
						case OrderStatus.NotDelivered:
							return 10;
						case OrderStatus.Closed:
							return 11;
						default:
							return 999;
					};
				}
			}
		}
	}
}
