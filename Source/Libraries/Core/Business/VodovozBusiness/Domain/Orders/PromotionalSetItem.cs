using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Project.Repositories;
using QS.Utilities;
using QS.Utilities.Text;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки промонабора",
		Nominative = "строка промонабора")]
	[HistoryTrace]
	public class PromotionalSetItem : PropertyChangedBase, IDomainObject, ICalculatingPriceWithManyDiscounts
	{
		private PromotionalSet _promoSet;
		private Nomenclature _nomenclature;
		private int _count = -1;
		private decimal _discount;
		private decimal _discountMoney;
		private bool _isDiscountInMoney;

		public PromotionalSetItem()
		{
		}

		#region Cвойства

		public virtual int Id { get; set; }

		[Display(Name = "Рекламный набор")]
		public virtual PromotionalSet PromoSet
		{
			get => _promoSet;
			set => SetField(ref _promoSet, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
		
		[Display(Name = "Количество")]
		public virtual int Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}
		
		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount
		{
			get => _discount;
			set
			{
				if(value > 100)
				{
					value = 100;
				}

				if(value < 0)
				{
					value = 0;
				}

				if(SetField(ref _discount, value))
				{
					OnPropertyChanged(nameof(ManualChangingDiscount));
				}
			}
		}
		
		[Display(Name = "Скидка в рублях")]
		public virtual decimal DiscountMoney
		{
			get => _discountMoney;
			set
			{
				if(SetField(ref _discountMoney, value))
				{
					OnPropertyChanged(nameof(ManualChangingDiscount));
				}
			}
		}

		[Display(Name = "Установлена ли скидка в рублях")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			set
			{
				if(SetField(ref _isDiscountInMoney, value))
				{
					OnPropertyChanged(nameof(ManualChangingDiscount));
				}
			}
		}

		decimal ICalculatingPriceWithManyDiscounts.Count => Count;
		public virtual bool IsFixedPrice => false;
		public virtual DiscountReason DiscountReason => null;
		
		public virtual IEnumerable<IDiscountData> Discounts => new []
		{
			DiscountData.Create(IsDiscountInMoney, Discount, DiscountReason)
		};

		public virtual decimal ManualChangingDiscount
		{
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set
			{
				if(IsDiscountInMoney)
				{
					DiscountMoney = value;
					Discount = 0;
				}
				else
				{
					Discount = value;
					DiscountMoney = 0;
				}
			}
		}

		#endregion

		public virtual string Title => string.Format(
			"{0} №{1}: {2} ед. {3} со скидкой {4}{5}",
			TypeOfEntityRepository.GetRealName(typeof(PromotionalSetItem)).StringToTitleCase(),
			PromoSet.Id,
			Count,
			Nomenclature.Name,
			IsDiscountInMoney ? DiscountMoney : Discount,
			IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%"
		);

		/// <summary>
		/// Сумма строки промо набора
		/// </summary>
		public virtual decimal Sum
		{
			get
			{
				var sum = SumWithoutDiscount;
				
				var discountMoney = IsDiscountInMoney
				? DiscountMoney
				: Math.Round(sum * Discount / 100, 2);
				
				return sum - discountMoney;
			}
		}

		private decimal SumWithoutDiscount => Math.Round(Count * Nomenclature.GetPrice(Count, false), 2);
	}
}
