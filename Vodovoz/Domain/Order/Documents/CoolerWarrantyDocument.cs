using System;
using System.Collections.Generic;
using QSSupportLib;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Orders.Documents
{
	public class CoolerWarrantyDocument:OrderDocument
	{
		
		#region implemented abstract members of OrderDocument

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Гарантийный талон на кулера №{0}", Order.Id),
				Identifier = "Documents.CoolerWarranty",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization])}
				}
			};
		}

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.CoolerWarranty;
			}
		}		

		public override string DocumentDate {
			get { return String.Format ("от {0}", Order.DeliveryDate.ToShortDateString ()); }
		}

		#endregion

		public override string Name { get { return String.Format ("Гарантийный талон на кулера №{0}", Order.Id); } }

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}
	}
}

