using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.Rent
{
	public class VodovozWebSiteFreeRentPackageOnlineParameters : FreeRentPackageOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForVodovozWebSite;
	}
}
