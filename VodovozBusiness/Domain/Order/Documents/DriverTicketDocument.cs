using System;
using System.Collections.Generic;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class DriverTicketDocument : OrderDocument
	{
		/// <summary>
		/// Тип документа используемый для маппинга для DiscriminatorValue
		/// а также для определния типа в нумераторе типов документов.
		/// Создано для определения этого значения только в одном месте.
		/// </summary>
		public static new string OrderDocumentTypeValue { get => "DriverTicket"; }

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentTypeValues[OrderDocumentTypeValue];
			}
		}

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Талон водителю {0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.DriverTicket",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id }
				}
			};
		}

		#endregion

		public override string Name { get { return String.Format ("Талон водителю №{0}", Order.Id); } }

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

		public override DocumentOrientation Orientation
		{
			get
			{
				return DocumentOrientation.Landscape;
			}
		}
	}
}

