using System;
using System.Collections.Generic;
using System.Globalization;
using QS.Print;
using QS.Report;

namespace Vodovoz.Domain.Orders.Documents
{
	public class ShetFacturaDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		private static readonly DateTime _edition2017LastDate = Convert.ToDateTime("2021-06-30T23:59:59", CultureInfo.CreateSpecificCulture("ru-RU"));

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.ShetFactura;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var identifier = Order.DeliveryDate <= _edition2017LastDate ? "Documents.ShetFactura2017Edition" : "Documents.ShetFactura";
			return new ReportInfo {
				Title = String.Format("Счет-Фактура {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = identifier,
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => String.Format("Счет-Фактура №{0}", Order.Id);

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;
	}
}

