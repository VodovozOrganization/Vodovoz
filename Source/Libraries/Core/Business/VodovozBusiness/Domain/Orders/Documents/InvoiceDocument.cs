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
					{ "need_terminal", Order.PaymentType == PaymentType.TerminalQR }
			}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => string.Format("Накладная №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		int _copiesToPrint = 1;
		public override int CopiesToPrint {
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		#region Свои свойства

		bool _withoutAdvertising;
		[Display(Name = "Без рекламы")]
		public virtual bool WithoutAdvertising {
			get => _withoutAdvertising;
			set => SetField(ref _withoutAdvertising, value, () => WithoutAdvertising);
		}

		bool _hideSignature = true;
		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature {
			get => _hideSignature;
			set => SetField(ref _hideSignature, value, () => HideSignature);
		}

		#endregion
	}
}
