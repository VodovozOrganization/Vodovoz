using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSReport;
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
				Title = String.Format ("Счет №{0} от {1:d}", Order.Id, Order.BillDate),
				Identifier = "Documents.Bill",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization]) },
					{ "hide_signature", HideSignature }
				}
			};
		}

		public override QSReport.ReportInfo GetReportInfoForPreview()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Счет №{0} от {1:d}", Order.Id, Order.BillDate),
				Identifier = "Documents.Bill",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization]) },
					{ "hide_signature", HideSignature }
				}
			};
		}

		#endregion

		public override string Name {
			get { return String.Format ("Счет №{0}", Order.Id); }
		}			

		public override DateTime? DocumentDate {
			get { return Order?.BillDate; }
		}
			
		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

		#region Свои свойства

		private bool hideSignature;

		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature {
			get { return hideSignature; }
			set { SetField(ref hideSignature, value, () => HideSignature); }
		}

		#endregion
	}
}

