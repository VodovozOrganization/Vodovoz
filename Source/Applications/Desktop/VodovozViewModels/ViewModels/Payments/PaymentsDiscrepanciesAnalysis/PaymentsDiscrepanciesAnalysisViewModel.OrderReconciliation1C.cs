using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Продажа из акта сверки 1С, распознанная как заказ.
		/// </summary>
		public class OrderReconciliation1C
		{
			/// <summary>
			/// Номер заказа.
			/// </summary>
			public int OrderId { get; set; }

			/// <summary>
			/// Дата доставки заказа из документа 1С.
			/// </summary>
			public DateTime? OrderDeliveryDate { get; set; }

			/// <summary>
			/// Сумма заказа по акту сверки 1С.
			/// </summary>
			public decimal OrderSum { get; set; }

			/// <summary>
			/// Наименование документа 1С.
			/// </summary>
			public string DocumentName { get; set; }

			/// <summary>
			/// Признак того, что строка акта 1С распознана как заказ.
			/// </summary>
			public bool IsRecognizedOrder { get; set; }
		}
	}
}
