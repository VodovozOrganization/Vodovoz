using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "параметры номенклатуры для ИИ бота",
		Accusative = "параметры номенклатуры для ИИ бота",
		Nominative = "параметры номенклатуры для ИИ бота")]
	public class AiBotNomenclatureOnlineParameters : NomenclatureOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForAiBot;
	}
}
