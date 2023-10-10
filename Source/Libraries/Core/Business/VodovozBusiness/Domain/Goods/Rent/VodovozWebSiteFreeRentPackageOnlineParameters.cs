using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.Rent
{
	public class VodovozWebSiteFreeRentPackageOnlineParameters : FreeRentPackageOnlineParameters
	{
		public override NomenclatureOnlineParameterType Type => NomenclatureOnlineParameterType.ForVodovozWebSite;
	}
}
