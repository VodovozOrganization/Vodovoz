using System;
using Vodovoz.Domain.Service;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderDoneWorkDocument:OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.DoneWorkReport;
			}
		}

		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = Name,
				Identifier = "DoneWorkReport",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "service_claim_id",ServiceClaim.Id }
				}
			};
		}

		#endregion

		ServiceClaim serviceClaim;

		[Display (Name = "Заявка на сервис")]
		public virtual ServiceClaim ServiceClaim {
			get { return serviceClaim; }
			set { SetField (ref serviceClaim, value, () => ServiceClaim); }
		}

		public override string Name {
			get { return String.Format ("Акт выполненных работ №{0}", serviceClaim.Id); }
		}

		public override string DocumentDate {
			get { return String.Format ("от {0}", serviceClaim.ServiceStartDate ); }
		}
	}
}

