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
		public PromotionalSetItem() { }

		#region Cвойства

		public virtual int Id { get; set; }

		PromotionalSet promoSet;
		[Display(Name = "Промонабор")]
		public virtual PromotionalSet PromoSet {
			get => promoSet;
			set => SetField(ref promoSet, value, () => PromoSet);
		}

		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value, () => Nomenclature);
		}

		int count = -1;
		[Display(Name = "Количество")]
		public virtual int Count {
			get => count;
			set => SetField(ref count, value, () => Count);
		}

		decimal discount;
		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount {
			get => discount;
			set {
				if(value > 100)
					value = 100;
				if(value < 0)
					value = 0;
				if(SetField(ref discount, value, () => Discount)) {
					OnPropertyChanged(nameof(ManualChangingDiscount));
				}
			}
		}

		private decimal discountMoney;
		[Display(Name = "Скидка в рублях")]
		public virtual decimal DiscountMoney {
			get => discountMoney;
			set {
				if(SetField(ref discountMoney, value)) {
					OnPropertyChanged(nameof(ManualChangingDiscount));
				}
			}
		}

		private bool isDiscountInMoney;
		[Display(Name = "Установлена ли скидка в рублях")]
		public virtual bool IsDiscountInMoney {
			get => isDiscountInMoney;
			set {
				if(SetField(ref isDiscountInMoney, value, () => IsDiscountInMoney)) {
					OnPropertyChanged(nameof(ManualChangingDiscount));
				}
			}
		}

		public virtual decimal ManualChangingDiscount {
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set {
				if(IsDiscountInMoney) {
					DiscountMoney = value;
				} else {
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
