using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "онлайн цены номенклатуры для ИИ бота",
		Accusative = "онлайн цену номенклатуры для ИИ бота",
		Nominative = "онлайн цена номенклатуры для ИИ бота")]
	public class AiBotNomenclatureOnlinePrice : NomenclatureOnlinePrice
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForAiBot;
	}
}
