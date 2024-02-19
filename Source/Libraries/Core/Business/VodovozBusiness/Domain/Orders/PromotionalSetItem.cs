using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Project.Repositories;
using QS.Utilities.Text;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки промонабора",
		Nominative = "строка промонабора")]
	[HistoryTrace]
	public class PromotionalSetItem : PropertyChangedBase, IDomainObject
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

		public virtual decimal ManualChangingDiscount
		{
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set
			{
				if(IsDiscountInMoney)
				{
					DiscountMoney = value;
				}
				else
				{
					Discount = value;
				}
			}
		}

		#endregion

		public virtual string Title => string.Format(
			"{0} №{1}: {2} ед. {3} со скидкой {4}%",
			TypeOfEntityRepository.GetRealName(typeof(PromotionalSetItem)).StringToTitleCase(),
			PromoSet.Id,
			Count,
			Nomenclature.Name,
			Discount
		);
	}
}
