using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	[HistoryTrace]
	public class OrderItemEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private decimal _price;
		private bool _isUserPrice;
		private decimal? _actualCount;
		private decimal? _includeNDS;
		private bool _isDiscountInMoney;
		private decimal _discount;
		private decimal? _originalDiscount;
		private decimal _discountMoney;
		private decimal? _originalDiscountMoney;
		private decimal _discountByStock;
		private decimal? _valueAddedTax;
		private bool _isAlternativePrice;
		private bool _isFixedPrice;
		private int _rentCount;
		private int _rentEquipmentCount;
		private decimal _count = -1;

		private OrderRentType _rentType;
		private OrderItemRentSubType _orderItemRentSubType;

		private OrderEntity _order;
		private OrderItemEntity _copiedFromUndelivery;
		private NomenclatureEntity _nomenclature;

		protected OrderItemEntity()
		{
		}

		#region Свойства

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => _id = value;
		}

		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			protected set => SetField(ref _price, value);
		}

		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice
		{
			get => _isUserPrice;
			set => SetField(ref _isUserPrice, value);
		}

		public virtual decimal? ActualCount
		{
			get => _actualCount;
			protected set => SetField(ref _actualCount, value);
		}

		[Display(Name = "Включая НДС")]
		public virtual decimal? IncludeNDS
		{
			get => _includeNDS;
			set => SetField(ref _includeNDS, value);
		}

		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			protected set => SetField(ref _isDiscountInMoney, value);
		}

		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount
		{
			get => _discount;
			protected set => SetField(ref _discount, value);
		}

		[Display(Name = "Процент скидки на товар которая была установлена до отмены заказа")]
		public virtual decimal? OriginalDiscount
		{
			get => _originalDiscount;
			set => SetField(ref _originalDiscount, value);
		}

		[Display(Name = "Скидка на товар в деньгах")]
		public virtual decimal DiscountMoney
		{
			get => _discountMoney;
			protected set => SetField(ref _discountMoney, value);
		}

		[Display(Name = "Скидки на товар которая была установлена до отмены заказа")]
		public virtual decimal? OriginalDiscountMoney
		{
			get => _originalDiscountMoney;
			set => SetField(ref _originalDiscountMoney, value);
		}

		[Display(Name = "Скидка по акции")]
		public virtual decimal DiscountByStock
		{
			get => _discountByStock;
			set => SetField(ref _discountByStock, value);
		}

		[Display(Name = "НДС на момент создания заказа")]
		public virtual decimal? ValueAddedTax
		{
			get => _valueAddedTax;
			set => SetField(ref _valueAddedTax, value);
		}

		[Display(Name = "Альтернативная цена?")]
		public virtual bool IsAlternativePrice
		{
			get => _isAlternativePrice;
			set => SetField(ref _isAlternativePrice, value);
		}

		[Display(Name = "Установлена фиксированная цена?")]
		public virtual bool IsFixedPrice
		{
			get => _isFixedPrice;
			set => SetField(ref _isFixedPrice, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			protected set => SetField(ref _count, value);
		}

		#region Аренда

		[Display(Name = "Тип аренды")]
		public virtual OrderRentType RentType
		{
			get => _rentType;
			set => SetField(ref _rentType, value);
		}

		[Display(Name = "Подтип позиции аренды")]
		public virtual OrderItemRentSubType OrderItemRentSubType
		{
			get => _orderItemRentSubType;
			set => SetField(ref _orderItemRentSubType, value);
		}

		[Display(Name = "Количество аренды (дни/месяцы)")]
		public virtual int RentCount
		{
			get => _rentCount;
			protected set => SetField(ref _rentCount, value);
		}

		[Display(Name = "Количество оборудования для аренды")]
		public virtual int RentEquipmentCount
		{
			get => _rentEquipmentCount;
			set => SetField(ref _rentEquipmentCount, value);
		}

		#endregion Аренда

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		public virtual OrderItemEntity CopiedFromUndelivery
		{
			get => _copiedFromUndelivery;
			set => SetField(ref _copiedFromUndelivery, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		#endregion

		public virtual decimal ReturnedCount => Count - ActualCount ?? 0;

		public virtual bool IsDelivered => ReturnedCount == 0;

		public virtual decimal ManualChangingOriginalDiscount =>
			IsDiscountInMoney ? (OriginalDiscountMoney ?? 0) : (OriginalDiscount ?? 0);
		
		private decimal GetPercentDiscount() => IsDiscountInMoney ? (100 * DiscountMoney) / (Price * CurrentCount) : Discount;

		public virtual decimal CurrentNDS => IncludeNDS ?? 0;
		public virtual decimal PriceWithoutVat => Math.Round((Price * CurrentCount - CurrentNDS - DiscountMoney) / CurrentCount, 2);
		public virtual decimal SumWithoutVat => Math.Round(Price * CurrentCount - CurrentNDS - DiscountMoney, 2);

		public virtual decimal CurrentCount => ActualCount ?? Count;

		public virtual decimal Sum => Math.Round(Price * Count - DiscountMoney, 2);

		public virtual decimal ActualSum => Math.Round(Price * CurrentCount - DiscountMoney, 2);

		public virtual decimal OriginalSum => Math.Round(Price * Count - (OriginalDiscountMoney ?? 0), 2);

		public virtual bool RentVisible => OrderItemRentSubType == OrderItemRentSubType.RentServiceItem;
	}
}
