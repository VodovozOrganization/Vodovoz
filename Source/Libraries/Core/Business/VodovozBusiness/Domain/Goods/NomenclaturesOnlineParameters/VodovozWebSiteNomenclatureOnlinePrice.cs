using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "онлайн цены номенклатуры для сайта ВВ",
		Accusative = "онлайн цену номенклатуры для сайта ВВ",
		Nominative = "онлайн цена номенклатуры для сайта ВВ")]
	public class VodovozWebSiteNomenclatureOnlinePrice : NomenclatureOnlinePrice
	{
		public override NomenclatureOnlineParameterType Type => NomenclatureOnlineParameterType.ForVodovozWebSite;
	}
}
