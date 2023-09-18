using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods
{
	public class MobileAppNomenclatureOnlineCatalog : NomenclatureOnlineCatalog
	{
		public override NomenclatureOnlineParameterType Type => NomenclatureOnlineParameterType.ForMobileApp;
	}
}
