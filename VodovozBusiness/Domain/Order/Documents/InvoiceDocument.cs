using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument : OrderDocument, IAdvertisable, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Invoice;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("Накладная №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.Invoice",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "without_advertising",  WithoutAdvertising },
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name { get { return String.Format("Накладная №{0}", Order.Id); } }

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

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

