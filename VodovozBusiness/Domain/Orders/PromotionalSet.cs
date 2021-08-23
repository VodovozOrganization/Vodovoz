using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings.Utilities;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Services;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "рекламные наборы",
		Nominative = "рекламный набор",
		Prepositional = "рекламном наборе",
		PrepositionalPlural = "рекламных наборах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class PromotionalSet : BusinessObjectBase<PromotionalSet>, IDomainObject, IValidatableObject
	{
		#region Cвойства

		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название набора")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}


		DiscountReason promoSetDiscountReason;
		[Display(Name = "Основание скидки набора")]
		public virtual DiscountReason PromoSetDiscountReason {
			get => promoSetDiscountReason;
			set => SetField(ref promoSetDiscountReason, value, () => PromoSetDiscountReason);
		}

		DateTime createDate;
		[Display(Name = "Дата создания")]
		[IgnoreHistoryTrace]
		public virtual DateTime CreateDate {
			get => createDate;
			set => SetField(ref createDate, value, () => CreateDate);
		}

		bool isArchive;
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}

		bool canEditNomenclatureCount;
		[Display(Name = "Можно менять количество номенклатур")]
		public virtual bool CanEditNomenclatureCount {
			get => canEditNomenclatureCount;
			set => SetField(ref canEditNomenclatureCount, value, () => CanEditNomenclatureCount);
		}

		bool canBeAddedWithOtherPromoSets;
		[Display(Name = "Может быть добавлен вместе с другими промонаборами")]
		public virtual bool CanBeAddedWithOtherPromoSets {
			get => canBeAddedWithOtherPromoSets;
			set => SetField(ref canBeAddedWithOtherPromoSets, value, () => CanBeAddedWithOtherPromoSets);
		}

		IList<PromotionalSetItem> promotionalSetItems = new List<PromotionalSetItem>();
		[Display(Name = "Строки рекламного набора")]
		public virtual IList<PromotionalSetItem> PromotionalSetItems {
			get => promotionalSetItems;
			set => SetField(ref promotionalSetItems, value, () => PromotionalSetItems);
		}

		GenericObservableList<PromotionalSetItem> observablePromotionalSetItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSetItem> ObservablePromotionalSetItems {
			get {
				if(observablePromotionalSetItems == null)
					observablePromotionalSetItems = new GenericObservableList<PromotionalSetItem>(promotionalSetItems);
				return observablePromotionalSetItems;
			}
		}

		IList<Order> orders = new List<Order>();
		[Display(Name = "Использован для заказов")]
		public virtual IList<Order> Orders {
			get => orders;
			set => SetField(ref orders, value, () => Orders);
		}

		GenericObservableList<Order> observableOrders;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Order> ObservableOrders {
			get {
				if(observableOrders == null)
					observableOrders = new GenericObservableList<Order>(Orders);
				return observableOrders;
			}
		}

		#endregion

		public virtual string Title => string.Format("Рекламный набор №{0} \"{1}\"", Id, Name);
		public virtual string ShortTitle => string.Format("Промо-набор \"{0}\"", Name);

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(Name))
				yield return new ValidationResult(
					"Необходимо выбрать название набора",
					new[] { this.GetPropertyName(o => o.Name) }
				);
			if((!PromotionalSetItems.Any() || PromotionalSetItems.Any(i => i.Count <= 0)))
				yield return new ValidationResult(
					"Необходимо выбрать номенклатуру",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
			if(PromotionalSetItems.Any(i => i.Count == 0))
				yield return new ValidationResult(
					"Необходимо выбрать количество номенклатур, отличное от нуля",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
			if(PromotionalSetItems.Any(i => i.Discount < 0 || i.Discount > 100))
				yield return new ValidationResult(
					"Скидка не может быть меньше 0 или больше 100%",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
			if(PromotionalSetItems.Any(i => i.Discount != 0) && PromoSetDiscountReason == null)
				yield return new ValidationResult(
					"При ненулевой скидке хотя бы на одну номенклатуру необходимо выбрать основание скидки",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
			if(PromotionalSetItems.Any(i => i.Discount != 0 &&
				PromotionalSetActions.OfType<PromotionalSetActionFixPrice>().Select(a => a.Nomenclature.Id).Contains(i.Nomenclature.Id)))
				yield return new ValidationResult(
					"Нельзя выбрать скидку на номенклатуру, для которой уже была создана фиксированная цена",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
		}

		#endregion

		IList<PromotionalSetActionBase> promotionalSetActions = new List<PromotionalSetActionBase>();
		public virtual IList<PromotionalSetActionBase> PromotionalSetActions {
			get => promotionalSetActions;
			set => SetField(ref promotionalSetActions, value, () => PromotionalSetActions);
		}

		GenericObservableList<PromotionalSetActionBase> observablePromotionalSetActions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSetActionBase> ObservablePromotionalSetActions {
			get {
				if(observablePromotionalSetActions == null)
					observablePromotionalSetActions = new GenericObservableList<PromotionalSetActionBase>(promotionalSetActions);
				return observablePromotionalSetActions;
			}
		}

		public virtual bool IsValidForOrder(Order order, IStandartNomenclatures standartNomenclatures)
		{
			return !PromotionalSetActions.Any(a => !a.IsValidForOrder(order, standartNomenclatures));
		}
	}

	public enum PromotionalSetActionType
	{
		[Display(Name = "Фиксированная цена")]
		FixedPrice
	}
}
