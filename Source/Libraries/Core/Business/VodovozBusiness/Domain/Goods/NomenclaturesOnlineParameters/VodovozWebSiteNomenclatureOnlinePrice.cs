using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "онлайн цены номенклатуры для сайта ВВ",
		Accusative = "онлайн цену номенклатуры для сайта ВВ",
		Nominative = "онлайн цена номенклатуры для сайта ВВ")]
	public class VodovozWebSiteNomenclatureOnlinePrice : NomenclatureOnlinePrice
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForVodovozWebSite;
	}
}
