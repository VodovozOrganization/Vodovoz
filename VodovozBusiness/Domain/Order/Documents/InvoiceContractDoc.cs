using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Print;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceContractDoc : OrderDocument, IAdvertisable, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public virtual QSReport.ReportInfo GetReportInfo()
		{
			return new QSReport.ReportInfo {
				Title = String.Format("Накладная №{0} от {1:d} (контрактная документация)", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.InvoiceContractDoc",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "without_advertising",  WithoutAdvertising },
				}
			};
		}

		public override OrderDocumentType Type => OrderDocumentType.InvoiceContractDoc;

		#endregion

		public override string Name => String.Format("Накладная №{0} (контрактная документация)", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		#region Свои свойства

		private bool withoutAdvertising;

		[Display(Name = "Без рекламы")]
		public virtual bool WithoutAdvertising {
			get { return withoutAdvertising; }
			set { SetField(ref withoutAdvertising, value, () => WithoutAdvertising); }
		}

		#endregion
	}
}