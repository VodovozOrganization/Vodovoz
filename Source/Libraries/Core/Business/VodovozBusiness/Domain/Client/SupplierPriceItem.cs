using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Project.Repositories;
using QS.Utilities.Text;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "цены на ТМЦ",
			Nominative = "цена на ТМЦ",
			Accusative = "цены на ТМЦ",
			Genitive = "цены на ТМЦ"
		)
	]
	[HistoryTrace]
	public class SupplierPriceItem : PropertyChangedBase, IDomainObject, ISupplierPriceNode
	{
		public virtual int Id { get; set; }

		Nomenclature nomenclatureToBuy;
		[Display(Name = "закупаемая ТМЦ")]
		public virtual Nomenclature NomenclatureToBuy {
			get => nomenclatureToBuy;
			set => SetField(ref nomenclatureToBuy, value);
		}

		SupplierPaymentType paymentType;
		[Display(Name = "Форма оплаты")]
		public virtual SupplierPaymentType PaymentType {
			get => paymentType;
			set => SetField(ref paymentType, value);
		}

		decimal price;
		[Display(Name = "Цена закупки")]
		public virtual decimal Price {
			get => price;
			set => SetField(ref price, value);
		}
		
		PaymentCondition paymentCondition;
		[Display(Name = "Условие оплаты")]
		public virtual PaymentCondition PaymentCondition {
			get => paymentCondition;
			set => SetField(ref paymentCondition, value);
		}

		DeliveryType deliveryType;
		[Display(Name = "Способ получения")]
		public virtual DeliveryType DeliveryType {
			get => deliveryType;
			set => SetField(ref deliveryType, value);
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		AvailabilityForSale availabilityForSale;
		[Display(Name = "Статус")]
		public virtual AvailabilityForSale AvailabilityForSale {
			get => availabilityForSale;
			set => SetField(ref availabilityForSale, value);
		}

		DateTime changingDate;
		[Display(Name = "Дата изменения")]
		public virtual DateTime ChangingDate {
			get => changingDate;
			set => SetField(ref changingDate, value);
		}

		Counterparty supplier;
		private VatRate _vatRate;

		[Display(Name = "Поставщик")]
		public virtual Counterparty Supplier {
			get => supplier;
			set => SetField(ref supplier, value);
		}

		#region Calculatable methods

		public virtual string Title {
			get {
				return string.Format(
					"{0} №{1}",
					TypeOfEntityRepository.GetRealName(GetType())?.StringToTitleCase(),
					Id
				);
			}
		}

		public virtual ISupplierPriceNode Parent { get; set; }
		public virtual IList<ISupplierPriceNode> Children { get; set; }

		public virtual bool IsEditable => true;
		public virtual string PosNr { get; set; } = string.Empty;

		#endregion Calculatable methods
	}

	public enum SupplierPaymentType
	{
		[Display(Name = "Наличная")]
		cash,
		[Display(Name = "Безналичная")]
		cashless,
		[Display(Name = "Бартер")]
		barter
	}

	public class SupplierPaymentTypeStringType : EnumStringType
	{
		public SupplierPaymentTypeStringType() : base(typeof(SupplierPaymentType)) { }
	}

	public enum PaymentCondition
	{
		[Display(Name = "Полная предоплата")]
		FullPrepayment,
		[Display(Name = "Частичная предоплата")]
		PartialPrepayment,
		[Display(Name = "Постоплата")]
		Postpay
	}

	public class PaymentConditionStringType : EnumStringType
	{
		public PaymentConditionStringType() : base(typeof(PaymentCondition)) { }
	}

	public enum DeliveryType
	{
		[Display(Name = "Доставка")]
		Delivery,
		[Display(Name = "Самовывоз")]
		SelfDelivery,
		[Display(Name = "Любой способ получения")]
		Any
	}

	public class DeliveryTypeStringType : EnumStringType
	{
		public DeliveryTypeStringType() : base(typeof(DeliveryType)) { }
	}

	public enum AvailabilityForSale
	{
		[Display(Name = "В продаже")]
		Available,
		[Display(Name = "Нет в наличии")]
		OutOfStock,
		[Display(Name = "Снят с продажи")]
		Discontinued
	}

	public class AvailabilityForSaleStringType : EnumStringType
	{
		public AvailabilityForSaleStringType() : base(typeof(AvailabilityForSale)) { }
	}
}
