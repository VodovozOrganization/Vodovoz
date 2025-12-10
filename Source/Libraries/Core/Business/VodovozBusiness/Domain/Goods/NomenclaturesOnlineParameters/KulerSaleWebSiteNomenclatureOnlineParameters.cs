using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры номенклатуры для сайта Кулер Сэйл",
		Accusative = "параметры номенклатуры для сайта Кулер Сэйл",
		Nominative = "параметры номенклатуры для сайта Кулер Сэйл")]
	public class KulerSaleWebSiteNomenclatureOnlineParameters : NomenclatureOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForKulerSaleWebSite;
	}
}
