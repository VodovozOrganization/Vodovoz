using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.Rent
{
	public class MobileAppFreeRentPackageOnlineParameters : FreeRentPackageOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForMobileApp;
	}
}
