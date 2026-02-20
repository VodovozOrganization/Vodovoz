using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры промонабора для ИИ Бота",
		Accusative = "параметров промонабора для ИИ Бота",
		Nominative = "параметры промонабора для ИИ Бота")]
	public class AiBotPromotionalSetOnlineParameters : PromotionalSetOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForAiBot;
	}
}
