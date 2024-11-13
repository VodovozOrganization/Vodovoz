using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument : PrintableOrderDocument, IPrintableRDLDocument, IAdvertisable, ISignableDocument
	{
		int _copiesToPrint = 1;
		bool _withoutAdvertising;
		bool _hideSignature = true;

		public override string Name => string.Format("Накладная №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Invoice;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		
		public virtual Dictionary<object, object> Parameters { get; set; }

		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = $"Накладная №{Order.Id} от {Order.DeliveryDate:d}";
			reportInfo.Identifier = "Documents.Invoice";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id },
				{ "without_advertising",  WithoutAdvertising },
				{ "hide_signature", HideSignature },
				{ "contactless_delivery", Order.ContactlessDelivery },
				{ "need_terminal", Order.PaymentType == PaymentType.Terminal }
			};
			return reportInfo;
		}
		#endregion

		public override int CopiesToPrint {
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		#region Свои свойства

		[Display(Name = "Без рекламы")]
		public virtual bool WithoutAdvertising {
			get => _withoutAdvertising;
			set => SetField(ref _withoutAdvertising, value);
		}

		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature {
			get => _hideSignature;
			set => SetField(ref _hideSignature, value);
		}

		#endregion
	}
}
