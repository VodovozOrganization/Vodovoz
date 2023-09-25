using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings.Utilities;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Goods.PromotionalSets
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "рекламные наборы",
		Nominative = "рекламный набор",
		Prepositional = "рекламном наборе",
		PrepositionalPlural = "рекламных наборах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class PromotionalSet : BusinessObjectBase<PromotionalSet>, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		#region Cвойства

		public virtual int Id { get; set; }

		string _name;
		[Display(Name = "Название набора")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value, () => Name);
		}

		DateTime _createDate;
		[Display(Name = "Дата создания")]
		[IgnoreHistoryTrace]
		public virtual DateTime CreateDate {
			get => _createDate;
			set => SetField(ref _createDate, value, () => CreateDate);
		}

		bool _isArchive;
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive {
			get => _isArchive;
			set => SetField(ref _isArchive, value, () => IsArchive);
		}

		private string _discountReasonInfo;
		[Display(Name = "Согласованная акция")]
		public virtual string DiscountReasonInfo
		{
			get => _discountReasonInfo;
			set => SetField(ref _discountReasonInfo, value);
		}

		bool _canEditNomenclatureCount;
		[Display(Name = "Можно менять количество номенклатур")]
		public virtual bool CanEditNomenclatureCount {
			get => _canEditNomenclatureCount;
			set => SetField(ref _canEditNomenclatureCount, value, () => CanEditNomenclatureCount);
		}

		bool _canBeAddedWithOtherPromoSets;
		[Display(Name = "Может быть добавлен вместе с другими промонаборами")]
		public virtual bool CanBeAddedWithOtherPromoSets {
			get => _canBeAddedWithOtherPromoSets;
			set => SetField(ref _canBeAddedWithOtherPromoSets, value, () => CanBeAddedWithOtherPromoSets);
		}

		bool _canBeReorderedWithoutRestriction;
		[Display(Name = "Можно заказывать повторно без ограничений")]
		public virtual bool CanBeReorderedWithoutRestriction
		{
			get => _canBeReorderedWithoutRestriction;
			set => SetField(ref _canBeReorderedWithoutRestriction, value);
		}

		IList<PromotionalSetItem> _promotionalSetItems = new List<PromotionalSetItem>();
		[Display(Name = "Строки рекламного набора")]
		public virtual IList<PromotionalSetItem> PromotionalSetItems {
			get => _promotionalSetItems;
			set => SetField(ref _promotionalSetItems, value, () => PromotionalSetItems);
		}

		GenericObservableList<PromotionalSetItem> _observablePromotionalSetItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSetItem> ObservablePromotionalSetItems {
			get {
				if(_observablePromotionalSetItems == null)
					_observablePromotionalSetItems = new GenericObservableList<PromotionalSetItem>(_promotionalSetItems);
				return _observablePromotionalSetItems;
			}
		}

		IList<Order> _orders = new List<Order>();
		[Display(Name = "Использован для заказов")]
		public virtual IList<Order> Orders {
			get => _orders;
			set => SetField(ref _orders, value, () => Orders);
		}

		GenericObservableList<Order> _observableOrders;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Order> ObservableOrders {
			get {
				if(_observableOrders == null)
					_observableOrders = new GenericObservableList<Order>(Orders);
				return _observableOrders;
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
			if(PromotionalSetItems.Any(i => i.Discount != 0 &&
				PromotionalSetActions.OfType<PromotionalSetActionFixPrice>().Select(a => a.Nomenclature.Id).Contains(i.Nomenclature.Id)))
				yield return new ValidationResult(
					"Нельзя выбрать скидку на номенклатуру, для которой уже была создана фиксированная цена",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
		}

		#endregion

		IList<PromotionalSetActionBase> _promotionalSetActions = new List<PromotionalSetActionBase>();
		public virtual IList<PromotionalSetActionBase> PromotionalSetActions {
			get => _promotionalSetActions;
			set => SetField(ref _promotionalSetActions, value, () => PromotionalSetActions);
		}

		GenericObservableList<PromotionalSetActionBase> _observablePromotionalSetActions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSetActionBase> ObservablePromotionalSetActions {
			get {
				if(_observablePromotionalSetActions == null)
					_observablePromotionalSetActions = new GenericObservableList<PromotionalSetActionBase>(_promotionalSetActions);
				return _observablePromotionalSetActions;
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
