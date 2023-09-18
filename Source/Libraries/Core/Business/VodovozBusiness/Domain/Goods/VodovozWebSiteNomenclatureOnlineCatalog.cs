using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using QS.HistoryLog;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Nominative = "Онлайн каталог сайта ВВ",
		NominativePlural = "Онлайн каталоги номенклатур сайта ВВ")]
	[HistoryTrace]
	public class VodovozWebSiteNomenclatureOnlineCatalog : NomenclatureOnlineCatalog
	{
		public override NomenclatureOnlineParameterType Type => NomenclatureOnlineParameterType.ForVodovozWebSite;
	}
}
