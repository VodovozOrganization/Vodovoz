using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using Vodovoz.Domain.Service;
using System.Collections.Generic;

namespace Vodovoz.Domain.Orders.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы заказа",
		Nominative = "документ заказа")]
	public abstract class OrderDocument : PropertyChangedBase, IDomainObject, IPrintableDocument
	{
		public virtual int Id { get; set; }

		Order order;

		[Display (Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		Order attachedToOrder;

		[Display (Name = "Заказ")]
		public virtual Order AttachedToOrder {
			get { return order; }
			set { SetField (ref attachedToOrder, value, () => AttachedToOrder); }
		}

		public abstract OrderDocumentType Type{ get; }

		public virtual string Name { get { return "Не указан"; } }

		public virtual string DocumentDate { get { return "Не указано"; } }

		#region IPrintableDocument implementation
		public virtual QSReport.ReportInfo GetReportInfo (){
			throw new NotImplementedException ();
		}

		public virtual PrinterType PrintType {
			get {
				return PrinterType.None;
			}
		}
		#endregion
	}
		

	public enum OrderDocumentType
	{
		[ItemTitleAttribute ("Доп. соглашение для заказа")]
		AdditionalAgreement,
		[ItemTitleAttribute ("Договор для заказа")]
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
		PumpWarranty
	}

}

