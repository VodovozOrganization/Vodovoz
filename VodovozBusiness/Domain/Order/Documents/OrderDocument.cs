using System;
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
	}


	public enum OrderDocumentType
	{
		[Display(Name = "Доп. соглашение для заказа")]
		AdditionalAgreement,
		[Display(Name = "Договор для заказа")]
		Contract,
		[DocumentOfOrder]
		[Display(Name = "Счет")]
		Bill,
		[DocumentOfOrder]
		[Display(Name = "Акт выполненных работ")]
		DoneWorkReport,
		[DocumentOfOrder]
		[Display(Name = "Акт приема-передачи оборудования")]
		EquipmentTransfer,
		[DocumentOfOrder]
		[Display(Name = "Накладная (нал.)")]
		Invoice,
		[DocumentOfOrder]
		[Display(Name = "Накладная (безденежно)")]
		InvoiceBarter,
		[DocumentOfOrder]
		[Display(Name = "УПД")]
		UPD,
		[Display(Name = "Гарантийный талон для кулеров")]
		CoolerWarranty,
		[Display(Name = "Гарантийный талон для помп")]
		PumpWarranty,
		[DocumentOfOrder]
		[Display(Name = "Талон водителю")]
		DriverTicket,
		[DocumentOfOrder]
		[Display(Name = "ТОРГ-12")]
		Torg12,
		[DocumentOfOrder]
		[Display(Name = "Счет-Фактура")]
		ShetFactura,
		[DocumentOfOrder]
		[Display(Name = "Акт возврата залога за бутыли")]
		RefundBottleDeposit,
		[DocumentOfOrder]
		[Display(Name = "Акт возврата залога за оборудование")]
		RefundEquipmentDeposit,
		[DocumentOfOrder]
		[Display(Name = "Акт передачи-возврата бутылей")]
		BottleTransfer
	}

	public interface ITemplateOdtDocument
	{
		void PrepareTemplate(IUnitOfWork uow);
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class DocumentOfOrderAttribute : Attribute
	{

	}
}

