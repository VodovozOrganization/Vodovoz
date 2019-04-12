using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Project.Repositories;
using QS.Utilities.Text;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки рекламного набора",
		Nominative = "строка рекламного набора")]
	[HistoryTrace]
	public class PromotionalSetItem : PropertyChangedBase, IDomainObject
	{
		public PromotionalSetItem() { }

		#region Cвойства

		public virtual int Id { get; set; }

		PromotionalSet promoSet;
		[Display(Name = "Рекламный набор")]
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
			set => SetField(ref discount, value, () => Discount);
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
