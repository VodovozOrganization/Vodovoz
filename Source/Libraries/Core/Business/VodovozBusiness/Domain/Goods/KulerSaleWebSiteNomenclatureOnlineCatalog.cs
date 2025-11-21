using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Nominative = "Онлайн каталог сайта Кулер Сэйл",
		NominativePlural = "Онлайн каталоги номенклатур сайта Кулер сэйл")]
	[HistoryTrace]
	public class KulerSaleWebSiteNomenclatureOnlineCatalog : NomenclatureOnlineCatalog
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForKulerSaleWebSite;
	}
}
