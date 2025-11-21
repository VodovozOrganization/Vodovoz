using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры промонабора для сайта ВВ",
		Accusative = "параметров промонабора для сайта ВВ",
		Nominative = "параметры промонабора для сайта ВВ")]
	public class VodovozWebSitePromotionalSetOnlineParameters : PromotionalSetOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForVodovozWebSite;
	}
}
