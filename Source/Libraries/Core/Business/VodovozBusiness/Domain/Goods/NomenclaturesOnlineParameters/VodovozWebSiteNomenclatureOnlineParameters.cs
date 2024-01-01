using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры номенклатуры для сайта ВВ",
		Accusative = "параметры номенклатуры для сайта ВВ",
		Nominative = "параметры номенклатуры для сайта ВВ")]
	public class VodovozWebSiteNomenclatureOnlineParameters : NomenclatureOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForVodovozWebSite;
	}
}
