using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "онлайн цены номенклатуры для сайта Кулер Сэйл",
		Accusative = "онлайн цену номенклатуры для сайта Кулер Сэйл",
		Nominative = "онлайн цена номенклатуры для сайта Кулер Сэйл")]
	public class KulerSaleWebSiteNomenclatureOnlinePrice : NomenclatureOnlinePrice
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForKulerSaleWebSite;
	}
}
