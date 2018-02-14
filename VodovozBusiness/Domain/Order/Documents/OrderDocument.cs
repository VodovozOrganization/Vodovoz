using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using QSOrmProject;
using QSReport;

namespace Vodovoz.Domain.Orders.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы заказа",
		Nominative = "документ заказа")]
	public abstract class OrderDocument : PropertyChangedBase, IDomainObject, IPrintableDocument
	{
		public static string OrderDocumentTypeValue { get; }

		public virtual int Id { get; set; }

		Order order;

		/// <summary>
		/// Заказ для которого создавался документ
		/// </summary>
		/// <value>The order.</value>
		[Display (Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		Order attachedToOrder;

		/// <summary>
		/// Заказ в котором будет отображатся этот документ. 
		/// (в котором везется этот документ клиенту, может не совпадать с заказом
		/// для которого создавался)
		/// </summary>
		[Display (Name = "Заказ")]
		public virtual Order AttachedToOrder {
			get { return attachedToOrder; }
			set { SetField (ref attachedToOrder, value, () => AttachedToOrder); }
		}

		public abstract OrderDocumentType Type{ get; }

		public virtual string Name { get { return "Не указан"; } }

		public abstract DateTime? DocumentDate { get; }

		public virtual string DocumentDateText { get { return DocumentDate?.ToShortDateString() ?? "не указана"; } }

		#region IPrintableDocument implementation
		public virtual QSReport.ReportInfo GetReportInfo (){
			throw new NotImplementedException ();
		}

		public virtual QSReport.ReportInfo GetReportInfoForPreview(){
			return GetReportInfo ();
		}

		public virtual PrinterType PrintType {
			get {
				return PrinterType.None;
			}
		}

		public virtual DocumentOrientation Orientation{
			get{
				return DocumentOrientation.Portrait;
			}
		}
		#endregion

		/// <summary>
		/// Связывает указанные в каждом классе типы документов с нумератором этих типов
		/// </summary>
		public static Dictionary<string, OrderDocumentType> OrderDocumentTypeValues {
			get {
				var result = new Dictionary<string, OrderDocumentType>();
				result.Add(OrderAgreement.OrderDocumentTypeValue, OrderDocumentType.AdditionalAgreement);
				result.Add(OrderContract.OrderDocumentTypeValue, OrderDocumentType.Contract);
				result.Add(BillDocument.OrderDocumentTypeValue, OrderDocumentType.Bill);
				result.Add(CoolerWarrantyDocument.OrderDocumentTypeValue, OrderDocumentType.CoolerWarranty);
				result.Add(DoneWorkDocument.OrderDocumentTypeValue, OrderDocumentType.DoneWorkReport);
				result.Add(EquipmentTransferDocument.OrderDocumentTypeValue, OrderDocumentType.EquipmentTransfer);
				result.Add(InvoiceBarterDocument.OrderDocumentTypeValue, OrderDocumentType.InvoiceBarter);
				result.Add(InvoiceDocument.OrderDocumentTypeValue, OrderDocumentType.Invoice);
				result.Add(PumpWarrantyDocument.OrderDocumentTypeValue, OrderDocumentType.PumpWarranty);
				result.Add(UPDDocument.OrderDocumentTypeValue, OrderDocumentType.UPD);
				result.Add(DriverTicketDocument.OrderDocumentTypeValue, OrderDocumentType.DriverTicket);
				result.Add(Torg12Document.OrderDocumentTypeValue, OrderDocumentType.Torg12);
				result.Add(ShetFacturaDocument.OrderDocumentTypeValue, OrderDocumentType.ShetFactura);
				return result;
			}
		}
	}

	public enum OrderDocumentType
	{
		[Display (Name = "Доп. соглашение для заказа")]
		AdditionalAgreement,
		[Display (Name = "Договор для заказа")]
		Contract,
		[Display (Name = "Счет")]
		Bill,
		[Display (Name = "Счет (Без печати и подписи)")]
		BillWithoutSignature,
		[Display (Name = "Акт выполненных работ")]
		DoneWorkReport,
		[Display (Name = "Акт приема-передачи оборудования")]
		EquipmentTransfer,
		[Display (Name = "Накладная (нал.)")]
		Invoice,
		[Display (Name = "Накладная (безденежно)")]
		InvoiceBarter,
		[Display (Name = "УПД")]
		UPD,
		[Display(Name="Гарантийный талон для кулеров")]
		CoolerWarranty,
		[Display(Name="Гарантийный талон для помп")]
		PumpWarranty,
		[Display(Name="Талон водителю")]
		DriverTicket,
		[Display(Name="ТОРГ-12")]
		Torg12,
		[Display(Name="Счет-Фактура")]
		ShetFactura
	}

	public interface ITemplateOdtDocument
	{
		void PrepareTemplate(IUnitOfWork uow);
	}
}

