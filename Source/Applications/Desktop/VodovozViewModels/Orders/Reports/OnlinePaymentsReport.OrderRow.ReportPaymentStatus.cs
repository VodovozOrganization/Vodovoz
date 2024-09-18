using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public partial class OnlinePaymentsReport
	{
		public partial class OrderRow
		{
			public enum ReportPaymentStatus
			{
				[Display(Name = "Оплачено")]
				Paid,
				[Display(Name = "Отсутствует оплата")]
				Missing,
				[Display(Name = "Переплата")]
				OverPaid,
				[Display(Name = "Недоплата")]
				UnderPaid
			}
		}
	}
}
