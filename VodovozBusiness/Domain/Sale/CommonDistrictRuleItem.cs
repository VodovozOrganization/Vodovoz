using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "стоимости доставки района",
		Nominative = "стоимость доставки района")]
	public class CommonDistrictRuleItem : DistrictRuleItemBase
	{
		public virtual string Title => $"{DeliveryPriceRule}, то цена {Price:C0}";

		public override object Clone()
		{
			var newCommonDistrictRuleItem = new CommonDistrictRuleItem {
				Price = Price,
				DeliveryPriceRule = DeliveryPriceRule
			};
			return newCommonDistrictRuleItem;
		}
	}
}
