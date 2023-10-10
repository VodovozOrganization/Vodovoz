using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.Rent
{
	public class MobileAppFreeRentPackageOnlineParameters : FreeRentPackageOnlineParameters
	{
		public override NomenclatureOnlineParameterType Type => NomenclatureOnlineParameterType.ForMobileApp;
	}
}
