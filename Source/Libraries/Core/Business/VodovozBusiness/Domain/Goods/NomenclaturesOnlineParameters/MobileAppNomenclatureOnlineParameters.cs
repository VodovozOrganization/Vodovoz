using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "параметры номенклатуры для мобильного приложения",
		Accusative = "параметры номенклатуры для мобильного приложения",
		Nominative = "параметры номенклатуры для мобильного приложения")]
	public class MobileAppNomenclatureOnlineParameters : NomenclatureOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForMobileApp;
	}
}
