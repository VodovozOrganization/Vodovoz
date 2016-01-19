using System;
using System.Collections.Generic;
using QSSupportLib;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Orders.Documents
{
	public class BillDocument:OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.Bill;
			}
		}
			
		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Счет №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Bill",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization]) },
					{ "hide_signature", false }
				}
			};
		}

		public override QSReport.ReportInfo GetReportInfoForPreview()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Счет №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Bill",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization]) },
					{ "hide_signature", true }
				}
			};
		}

		#endregion

		public override string Name {
			get { return String.Format ("Счет №{0}", Order.Id); }
		}			

		public override string DocumentDate {
			get { return String.Format ("от {0}", Order.DeliveryDate.ToShortDateString ()); }
		}
			
		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}
	}
}

