using QS.HistoryLog;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Nominative = "Онлайн каталог сайта ВВ",
		NominativePlural = "Онлайн каталоги номенклатур сайта ВВ")]
	[HistoryTrace]
	public class VodovozWebSiteNomenclatureOnlineCatalog : NomenclatureOnlineCatalog
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForVodovozWebSite;
	}
}
