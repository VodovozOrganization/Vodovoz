using Vodovoz.Core.Application.Orders.Delivery;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	public interface IDeliveryCostData
	{
		/// <summary>
		/// Стоимость доставки
		/// </summary>
		decimal? DeliveryPrice { get; }
		/// <summary>
		/// Сообщение по доставке
		/// </summary>
		string Message { get; }
		/// <summary>
		/// Количество бОльшего объема бутылей <see cref="IMaxVolumeTotalBottles"/>
		/// </summary>
		IMaxVolumeTotalBottles MaxVolumeTotalBottles { get; }
	}
}
