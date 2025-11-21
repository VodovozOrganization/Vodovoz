using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Nominative = "Онлайн каталог мобильного приложения",
		NominativePlural = "Онлайн каталоги номенклатур мобильного приложения")]
	[HistoryTrace]
	public class MobileAppNomenclatureOnlineCatalog : NomenclatureOnlineCatalog
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForMobileApp;
	}
}
