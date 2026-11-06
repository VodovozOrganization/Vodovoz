using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;

namespace VodovozBusiness.Domain.Goods.Rent
{
	public class AiBotFreeRentPackageOnlineParameters : FreeRentPackageOnlineParameters
	{
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForAiBot;
	}
}
