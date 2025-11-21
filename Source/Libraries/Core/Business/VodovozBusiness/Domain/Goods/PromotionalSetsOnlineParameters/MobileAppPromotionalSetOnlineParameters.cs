using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "параметры промонабора для мобильного приложения",
		Accusative = "параметров промонабора для мобильного приложения",
		Nominative = "параметры промонабора для мобильного приложения")]
	public class MobileAppPromotionalSetOnlineParameters : PromotionalSetOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForMobileApp;
	}
}
