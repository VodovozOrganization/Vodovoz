using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.Orders.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы заказа",
		Nominative = "документ заказа")]
	public abstract class OrderDocument : PropertyChangedBase, IDocument
	{
		public virtual int Id { get; set; }

		Order order;
		/// <summary>
		/// Заказ для которого создавался документ
		/// </summary>
		/// <value>The order.</value>
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		Order attachedToOrder;

		/// <summary>
		/// Заказ в котором будет отображатся этот документ. 
		/// (в котором везется этот документ клиенту, может не совпадать с заказом
		/// для которого создавался)
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual Order AttachedToOrder {
			get => attachedToOrder;
			set => SetField(ref attachedToOrder, value, () => AttachedToOrder);
		}
		public abstract string Name { get; }

		public abstract OrderDocumentType Type { get; }
		
		public abstract DateTime? DocumentDate { get; }
        
		public virtual string DocumentDateText => DocumentDate?.ToShortDateString() ?? "не указана";
	}


	public enum OrderDocumentType
	{
		[Display(Name = "Доп. соглашение для заказа")]
		AdditionalAgreement,
		[Display(Name = "Договор для заказа")]
		Contract,
		[Display(Name = "Доверенность М-2 для заказа")]
		M2Proxy,
		[DocumentOfOrder]
		[Display(Name = "Счет")]
		Bill,
		[DocumentOfOrder]
		[Display(Name = "Особый счет")]
		SpecialBill,
		[DocumentOfOrder]
		[Display(Name = "Счет без отгрузки на долг")]
		BillWSForDebt,
		[DocumentOfOrder]
		[Display(Name = "Счет без отгрузки на предоплату")]
		BillWSForAdvancePayment,
		[DocumentOfOrder]
		[Display(Name = "Счет без отгрузки на постоплату")]
		BillWSForPayment,
		[DocumentOfOrder]
		[Display(Name = "Акт выполненных работ")]
		DoneWorkReport,
		[DocumentOfOrder]
		[Display(Name = "Акт приема-передачи оборудования")]
		EquipmentTransfer,
		[DocumentOfOrder]
		[Display(Name = "Акт закрытия аренды")]
		EquipmentReturn,
		[DocumentOfOrder]
		[Display(Name = "Накладная (нал.)")]
		Invoice,
		[DocumentOfOrder]
		[Display(Name = "Накладная (безденежно)")]
		InvoiceBarter,
		[DocumentOfOrder]
		[Display(Name = "Накладная (контрактная документация)")]
		InvoiceContractDoc,
		[DocumentOfOrder]
		[Display(Name = "УПД")]
		UPD,
		[DocumentOfOrder]
		[Display(Name = "Особый УПД")]
		SpecialUPD,
		[DocumentOfOrder]
		[Display(Name = "Талон водителю")]
		DriverTicket,
		[DocumentOfOrder]
		[Display(Name = "ТОРГ-12")]
		Torg12,
		[DocumentOfOrder]
		[Display(Name = "Счет-Фактура")]
		ShetFactura,
		[Display(Name = "Сертификат продукции")]
		ProductCertificate,
		[DocumentOfOrder]
		[Display(Name = "Товарно-транспортная накладная")]
		TransportInvoice,
		[DocumentOfOrder]
		[Display(Name = "ТОРГ-2")]
		Torg2,
		[DocumentOfOrder]
		[Display(Name = "Лист сборки")]
		AssemblyList
	}

	public interface ITemplateOdtDocument
	{
		void PrepareTemplate(IUnitOfWork uow);
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class DocumentOfOrderAttribute : Attribute { }

	/// <summary>
	/// Интерфейс необходим для документов заказа, напротив которых должен быть крыжик
	/// "Без рекламы" в разделе "Документы" в диалоге заказа.
	/// </summary>
	public interface IAdvertisable
	{
		bool WithoutAdvertising { get; set; }
	}

	/// <summary>
	/// Интерфейс необходим для документов заказа, напротив которых должен быть крыжик
	/// "Без подписей и печати" в разделе "Документы" в диалоге заказа.
	/// </summary>
	public interface ISignableDocument
	{
		bool HideSignature { get; set; }
	}
}