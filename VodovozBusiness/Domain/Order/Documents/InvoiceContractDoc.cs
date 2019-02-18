using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceContractDoc : OrderDocument, IPrintableRDLDocument, IAdvertisable, ISignableDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.InvoiceContractDoc;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("Накладная №{0} от {1:d} (контрактная документация)", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.InvoiceContractDoc",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "without_advertising",  WithoutAdvertising },
					{ "hide_signature", HideSignature }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Накладная №{0} (контрактная документация)", Order.Id);

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