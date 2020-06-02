using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "стоимости доставки",
		Nominative = "стоимость доставки")]
	public class CommonDistrictRuleItem : DistrictRuleItemBase
	{
		public virtual string Title => $"{DeliveryPriceRule}, то цена {DeliveryPrice:C0}";
	}
}
