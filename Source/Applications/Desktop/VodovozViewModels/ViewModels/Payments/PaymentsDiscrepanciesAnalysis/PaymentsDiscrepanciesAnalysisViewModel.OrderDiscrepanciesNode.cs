using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Строка сверки заказа из акта 1С с заказом в ДВ.
		/// </summary>
		public class OrderDiscrepanciesNode
		{
			/// <summary>
			/// Номер заказа.
			/// </summary>
			public int OrderId { get; set; }

			/// <summary>
			/// Наименование документа 1С.
			/// </summary>
			public string DocumentName { get; set; }

			/// <summary>
			/// Дата доставки заказа в ДВ.
			/// </summary>
			public DateTime? OrderDeliveryDateInDatabase { get; set; }

			/// <summary>
			/// Дата доставки заказа по акту сверки 1С.
			/// </summary>
			public DateTime? OrderDeliveryDateInDocument { get; set; }

			/// <summary>
			/// Статус заказа в ДВ.
			/// </summary>
			public OrderStatus? OrderStatus { get; set; }

			/// <summary>
			/// Статус оплаты заказа в ДВ.
			/// </summary>
			public OrderPaymentStatus? OrderPaymentStatus { get; set; }

			/// <summary>
			/// Сумма заказа по акту сверки 1С.
			/// </summary>
			public decimal DocumentOrderSum { get; set; }

			/// <summary>
			/// Сумма заказа в ДВ.
			/// </summary>
			public decimal ProgramOrderSum { get; set; }

			/// <summary>
			/// Распределенная на заказ сумма платежей.
			/// </summary>
			public decimal AllocatedSum { get; set; }

			/// <summary>
			/// Признак того, что заказ есть в ДВ, но отсутствует в акте 1С.
			/// </summary>
			public bool IsMissingFromDocument { get; set; }

			/// <summary>
			/// Название клиента заказа в ДВ.
			/// </summary>
			public string OrderClientNameInDatabase { get; set; }

			/// <summary>
			/// ИНН клиента заказа в ДВ.
			/// </summary>
			public string OrderClientInnInDatabase { get; set; }

			/// <summary>
			/// Признак расхождения суммы заказа между актом 1С и ДВ.
			/// </summary>
			public bool OrderSumDiscrepancy => ProgramOrderSum != DocumentOrderSum;

			/// <summary>
			/// Дата доставки заказа для отображения в сверке.
			/// </summary>
			public DateTime? OrderDeliveryDate => OrderDeliveryDateInDatabase ?? OrderDeliveryDateInDocument;
		}
	}
}
