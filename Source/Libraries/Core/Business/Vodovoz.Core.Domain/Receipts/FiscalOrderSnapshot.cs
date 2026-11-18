using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace Vodovoz.Core.Domain.Receipts
{
	/// <summary>
	/// Снимок фискального состояния заказа для сравнения. Не сохраняется отдельно —
	/// строится из последней завершённой пачки Edo-документов или исходного Sale.
	/// </summary>
	public class FiscalOrderSnapshot
	{
		public int OrderId { get; set; }

		public int? SourceEdoFiscalDocumentId { get; set; }

		public int? OrganizationId { get; set; }

		public int? CounterpartyId { get; set; }

		public int? ContractId { get; set; }

		public int? CashboxId { get; set; }

		public PaymentType? PaymentType { get; set; }

		public string ClientInn { get; set; }

		public string Contact { get; set; }

		public DateTime? DeliveryDate { get; set; }

		public string FiscalDocumentNumber { get; set; }

		public DateTime? FiscalDocumentDate { get; set; }

		public decimal Sum { get; set; }

		public IList<FiscalOrderSnapshotItem> Items { get; set; } = new List<FiscalOrderSnapshotItem>();
	}
}
