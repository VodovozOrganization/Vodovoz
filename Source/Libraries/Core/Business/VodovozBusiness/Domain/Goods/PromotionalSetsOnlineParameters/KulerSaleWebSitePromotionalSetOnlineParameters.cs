using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры промонабора для сайта Кулер Сэйл",
		Accusative = "параметров промонабора для сайта Кулер Сэйл",
		Nominative = "параметры промонабора для сайта Кулер Сэйл")]
	public class KulerSaleWebSitePromotionalSetOnlineParameters : PromotionalSetOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForKulerSaleWebSite;
	}
}
