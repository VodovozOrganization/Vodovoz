using System;
using System.Collections.Generic;
using QSReport;
using QSSupportLib;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Orders.Documents
{
	public class BillDocument:OrderDocument
	{
		/// <summary>
		/// Тип документа используемый для маппинга для DiscriminatorValue
		/// а также для определния типа в нумераторе типов документов.
		/// Создано для определения этого значения только в одном месте.
		/// </summary>
		public static new string OrderDocumentTypeValue { get => "bill_document"; }

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentTypeValues[OrderDocumentTypeValue];
			}
		}
			
		public override QSReport.ReportInfo GetReportInfo ()
		{
			return new QSReport.ReportInfo {
				Title = String.Format ("Счет №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.Bill",
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
				Identifier = "Documents.Bill",
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

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
		}
			
		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}
	}
}

