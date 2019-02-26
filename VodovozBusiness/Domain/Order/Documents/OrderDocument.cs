using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
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

		public abstract OrderDocumentType Type { get; }

		public virtual string Name => "Не указан";

		public abstract DateTime? DocumentDate { get; }

		public virtual string DocumentDateText => DocumentDate?.ToShortDateString() ?? "не указана";

		public virtual PrinterType PrintType => PrinterType.None;

		public virtual DocumentOrientation Orientation => DocumentOrientation.Portrait;

		readonly OrderDocumentType[] typesForVariableQuantity = {
			OrderDocumentType.UPD,
			OrderDocumentType.SpecialUPD,
			OrderDocumentType.Torg12,
			OrderDocumentType.ShetFactura
		};

		int copiesToPrint = -1;
		public virtual int CopiesToPrint {
			get {
				if(copiesToPrint < 0 && typesForVariableQuantity.Contains(Type))
					return Order.DocumentType.Value == DefaultDocumentType.torg12 ? 1 : 2;
				return copiesToPrint;
			}
			set => copiesToPrint = value;
		}
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
		BottleTransfer,
		[Display(Name = "Сертификат продукции")]
		ProductCertificate
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