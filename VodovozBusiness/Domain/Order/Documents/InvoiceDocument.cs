using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument : OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo()
		{
			return new QSReport.ReportInfo {
				Title = String.Format("Накладная №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.Invoice",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "without_advertising",  WithoutAdvertising },
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.Invoice;
			}
		}

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

