using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[HistoryTrace]
	public class PromotionalSetActionFixPrice : PromotionalSetActionBase, IValidatableObject
	{
		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		decimal price;
		[Display(Name = "Фиксированная цена")]
		public virtual decimal Price {
			get { return price; }
			set { SetField(ref price, value, () => Price); }
		}

		public override string Title => $"Фиксированная цена {Price}р. на {Nomenclature.ShortOrFullName}";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Nomenclature == null)
				yield return new ValidationResult("Необходимо выбрать номенклатуру");
			if(Price < 0)
				yield return new ValidationResult("Фиксированная цена не может быть отрицательной");
			if(PromotionalSet.ObservablePromotionalSetActions.Cast<PromotionalSetActionFixPrice>().Any(a => a.Nomenclature == Nomenclature))
				yield return new ValidationResult("Фиксированная цена на такую номенклатуру уже создана");
		}
	}
}
