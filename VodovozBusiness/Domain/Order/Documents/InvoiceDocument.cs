using System;
using System.Collections.Generic;
using QSReport;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument : OrderDocument
	{
		/// <summary>
		/// Тип документа используемый для маппинга для DiscriminatorValue
		/// а также для определния типа в нумераторе типов документов.
		/// Создано для определения этого значения только в одном месте.
		/// </summary>
		public static new string OrderDocumentTypeValue { get => "invoice"; }

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentTypeValues[OrderDocumentTypeValue];
			}
		}

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Накладная №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = Order.PaymentType == PaymentType.barter ? "Documents.InvoiceBarter" : "Documents.Invoice",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id }
				}
			};
		}

		#endregion

		public override string Name { get { return String.Format ("Накладная №{0}",Order.Id); } }

		public override string DocumentDate {
			get { return String.Format ("от {0:d}", Order.DeliveryDate); }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

	}
}

