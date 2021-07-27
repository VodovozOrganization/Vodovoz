using System;
using System.Collections.Generic;
using System.Globalization;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class SpecialUPDDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		private static readonly DateTime _edition2017LastDate = Convert.ToDateTime("2021-06-30T23:59:59", CultureInfo.CreateSpecificCulture("ru-RU"));

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.SpecialUPD;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			var identifier = Order.DeliveryDate <= _edition2017LastDate ? "Documents.UPD2017Edition" : "Documents.UPD";
			return new ReportInfo {
				Title = String.Format("Особый УПД {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = identifier,
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "special", true }
				},
				RestrictedOutputPresentationTypes = RestrictedOutputPresentationTypes
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Особый УПД №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;

		int copiesToPrint = 2;
		public override int CopiesToPrint {
			get
			{
				if (Order.PaymentType == PaymentType.BeveragesWorld && Order.Client.UPDCount.HasValue)
					return Order.Client.UPDCount.Value;

				return copiesToPrint;
			}
			
			set => copiesToPrint = value;
		}
	}
}
