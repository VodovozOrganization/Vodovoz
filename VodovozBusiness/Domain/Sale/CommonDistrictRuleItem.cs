using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "стоимости доставки района",
		Nominative = "стоимость доставки района")]
	public class CommonDistrictRuleItem : DistrictRuleItemBase
	{
		public virtual string Title => $"{DeliveryPriceRule}, то цена {Price.ToString("C0", CultureInfo.CreateSpecificCulture("ru-RU"))}";

		SectorDeliveryRuleVersion _sectorDeliveryRuleVersion;
		[Display(Name = "Район доставки")]
		public virtual SectorDeliveryRuleVersion SectorDeliveryRuleVersion {
			get => _sectorDeliveryRuleVersion;
			set => SetField(ref _sectorDeliveryRuleVersion, value);
		}
		public override object Clone()
		{
			var newCommonDistrictRuleItem = new CommonDistrictRuleItem {
				Price = Price,
				DeliveryPriceRule = DeliveryPriceRule,
				SectorDeliveryRuleVersion = SectorDeliveryRuleVersion
			};
			return newCommonDistrictRuleItem;
		}
	}
}
