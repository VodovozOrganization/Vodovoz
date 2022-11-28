using System;
using Gamma.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport
{
	public class EdoUpdReportRow
	{
		public string Inn { get; set; }
		public string CounterpartyName { get; set; }
		public int OrderId { get; set; }
		public DateTime UpdDate { get; set; }
		public string UpdDateString => UpdDate.ToString("g");
		public string Gtin { get; set; }
		public decimal Count { get; set; }
		public decimal Price { get; set; }
		public decimal Sum { get; set; }
		public EdoDocFlowStatus? EdoDocFlowStatus { get; set; }
		public string EdoDocFlowStatusString => EdoDocFlowStatus?.GetEnumTitle() ?? "-";
		public string TrueMarkApiError { get; set; }
		public bool? IsTrueMarkApiSuccess { get; set; }
		public string TrueMarkApiErrorString => IsTrueMarkApiSuccess.HasValue && IsTrueMarkApiSuccess.Value ?   "Успешно" : "Не выводилось";
	}
}
