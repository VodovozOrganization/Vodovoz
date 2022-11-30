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
		public string UpdDateString => UpdDate.ToString("dd.MM.yyyy");
		public string Gtin { get; set; }
		public decimal Count { get; set; }
		public decimal Price { get; set; }
		public decimal Sum => Price * Count;
		public EdoDocFlowStatus? EdoDocFlowStatus { get; set; }
		public string EdoDocError { get; set; }

		public string EdoDocFlowStatusString => EdoDocFlowStatus.HasValue && EdoDocFlowStatus == Domain.Orders.Documents.EdoDocFlowStatus.Error 
			? EdoDocError 
			: EdoDocFlowStatus?.GetEnumTitle() ?? "Не отправлялось";

		public string TrueMarkApiError { get; set; }
		public bool? IsTrueMarkApiSuccess { get; set; }

		public string TrueMarkApiStatusString => IsTrueMarkApiSuccess.HasValue && IsTrueMarkApiSuccess.Value
			? "Успешно"
			: string.IsNullOrWhiteSpace(TrueMarkApiError)
				? "Не выводилось"
				: TrueMarkApiError;
	}
}
