using System;
using System.Collections.Generic;
using QSSupportLib;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Orders.Documents
{
	public class PumpWarrantyDocument:OrderDocument
	{
		#region implemented abstract members of OrderDocument
		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Гарантийный талон на помпы №{0}", Order.Id),
				Identifier = "PumpWarranty",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization])}
				}
			};
		}
		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.PumpWarranty;
			}
		}
		#endregion

		public override string Name { get { return "Гарантийный талон на помпы"; } }
	}
}

