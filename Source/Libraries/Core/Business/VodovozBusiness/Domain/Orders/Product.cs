using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public abstract class Product : PropertyChangedBase
	{
		private decimal _count = -1;
		private decimal? _actualCount;
		private decimal _price;
		private decimal _discountMoney;
		private decimal _discount;
		private bool _isDiscountInMoney;
		private Nomenclature _nomenclature;
		private PromotionalSet _promoSet;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			protected set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Добавлено из промонабора")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			protected set => SetField(ref _count, value);
		}
		
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			protected set => SetField(ref _price, value);
		}
		
		[Display(Name = "Скидка на товар в деньгах")]
		public virtual decimal DiscountMoney
		{
			get => _discountMoney;
			protected set => SetField(ref _discountMoney, value);
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
		
		public virtual decimal? ActualCount
		{
			get => _actualCount;
			protected set => SetField(ref _actualCount, value);
		}
		
		public virtual decimal CurrentCount => ActualCount ?? Count;
		public virtual decimal GetDiscount => IsDiscountInMoney ? DiscountMoney : Discount;
		public virtual decimal Sum => Math.Round(Price * Count - DiscountMoney, 2);
		public virtual decimal ActualSum => Math.Round(Price * CurrentCount - DiscountMoney, 2);
	}
}
