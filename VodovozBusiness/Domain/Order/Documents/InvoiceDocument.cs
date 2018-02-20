using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	public class InvoiceDocument : OrderDocument
	{
		/// <summary>
		/// Тип документа используемый для маппинга для DiscriminatorValue
		/// а также для определния типа в нумераторе типов документов.
		/// Создано для определения этого значения только в одном месте.
		/// </summary>
		public static new string OrderDocumentTypeValue { get => "invoice"; }

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentTypeValues[OrderDocumentTypeValue];
			}
		}

		public override QSReport.ReportInfo GetReportInfo()
		{
			return new QSReport.ReportInfo {
				Title = String.Format("Накладная №{0} от {1:d}", Order.Id, Order.DeliveryDate),
				Identifier = "Documents.Invoice",
				Parameters = new Dictionary<string, object> {
					{ "order_id",  Order.Id },
					{ "without_advertising",  WithoutAdvertising },
				}
			};
		}

		#endregion

		public override string Name { get { return String.Format("Накладная №{0}", Order.Id); } }

		public override DateTime? DocumentDate {
			get { return Order?.DeliveryDate; }
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.RDL;
			}
		}

		#region Свои свойства

		private bool withoutAdvertising;

		[Display(Name = "Без рекламы")]
		public virtual bool WithoutAdvertising {
			get { return withoutAdvertising; }
			set { SetField(ref withoutAdvertising, value, () => WithoutAdvertising); }
		}

		#endregion
	}
}

