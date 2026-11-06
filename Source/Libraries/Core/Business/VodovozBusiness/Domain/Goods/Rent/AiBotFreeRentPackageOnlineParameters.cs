using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;

namespace VodovozBusiness.Domain.Goods.Rent
{
	/// <summary>
	/// Онлайн параметры для ИИ бота
	/// </summary>
	public class AiBotFreeRentPackageOnlineParameters : FreeRentPackageOnlineParameters
	{
		/// <inheritdoc/>
		public override GoodsOnlineParameterType Type => GoodsOnlineParameterType.ForAiBot;
	}
}
