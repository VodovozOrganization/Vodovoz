using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceContractDoc : PrintableOrderDocument, IPrintableRDLDocument, IAdvertisable, ISignableDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.InvoiceContractDoc;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = String.Format("Накладная №{0} от {1:d} (контрактная документация)", Order.Id, Order.DeliveryDate);
			reportInfo.Identifier = "Documents.InvoiceContractDoc";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id },
				{ "without_advertising",  WithoutAdvertising },
				{ "hide_signature", HideSignature }
			};
			return reportInfo;
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Накладная №{0} (контрактная документация)", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		int copiesToPrint = 2;
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
