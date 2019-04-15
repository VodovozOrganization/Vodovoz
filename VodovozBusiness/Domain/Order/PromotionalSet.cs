using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings.Utilities;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

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
		public PromotionalSet() { }

		#region Cвойства

		public virtual int Id { get; set; }

		DiscountReason promoSetName;
		[Display(Name = "Название набора")]
		public virtual DiscountReason PromoSetName {
			get => promoSetName;
			set => SetField(ref promoSetName, value, () => PromoSetName);
		}

		DateTime createDate;
		[Display(Name = "Дата создания")]
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

		#endregion

		public virtual string Title => string.Format("Рекламный набор №{0} \"{1}\"", Id, PromoSetName.Name);
		public virtual string ShortTitle => string.Format("Промо-набор \"{0}\"", PromoSetName.Name);

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(PromoSetName == null)
				yield return new ValidationResult(
					"Необходимо выбрать скидку",
					new[] { this.GetPropertyName(o => o.PromoSetName) }
				);

			if(!PromotionalSetItems.Any() || PromotionalSetItems.Any(i => i.Count <= 0))
				yield return new ValidationResult(
					"Выберите номенклатуры и укажите их количества",
					new[] { this.GetPropertyName(o => o.PromotionalSetItems) }
				);
		}

		#endregion
	}
}
