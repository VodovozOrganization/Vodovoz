using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument : PrintableOrderDocument, IPrintableRDLDocument, IAdvertisable, ISignableDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Invoice;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			return new ReportInfo {
				Title = $"Накладная №{Order.Id} от {Order.DeliveryDate:d}",
				Identifier = "Documents.Invoice",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "without_advertising", WithoutAdvertising },
					{ "hide_signature", HideSignature },
					{ "contactless_delivery", Order.ContactlessDelivery },
					{ "payment_by_sms", Order.PaymentBySms },
					{ "payment_by_qr", Order.PaymentByQr },
					{ "need_terminal", Order.PaymentType == PaymentType.Terminal }
			}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Накладная №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		int copiesToPrint = 1;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}

		#region Свои свойства

		bool withoutAdvertising;
		[Display(Name = "Без рекламы")]
		public virtual bool WithoutAdvertising {
			get => withoutAdvertising;
			set => SetField(ref withoutAdvertising, value, () => WithoutAdvertising);
		}

		bool hideSignature = true;
		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature {
			get => hideSignature;
			set => SetField(ref hideSignature, value, () => HideSignature);
		}

		#endregion
	}
}
