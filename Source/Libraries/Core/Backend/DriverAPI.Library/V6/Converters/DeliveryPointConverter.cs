using DriverApi.Contracts.V6;
using Vodovoz.Domain.Client;

namespace DriverAPI.Library.V6.Converters
{
	/// <summary>
	/// Конвертер точки доставки
	/// </summary>
	public class DeliveryPointConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="deliveryPoint">Точка доставки из ДВ</param>
		/// <returns></returns>
		public AddressDto ExtractAPIAddressFromDeliveryPoint(DeliveryPoint deliveryPoint)
		{
			return new AddressDto()
			{
				City = deliveryPoint.City,
				Street = deliveryPoint.Street,
				Building = deliveryPoint.Building + deliveryPoint.Letter,
				Entrance = deliveryPoint.Entrance,
				Floor = deliveryPoint.Floor,
				Apartment = deliveryPoint.Room,
				DeliveryPointCategory = deliveryPoint.Category?.Name,
				EntranceType = deliveryPoint.EntranceType.ToString(),
				RoomType = deliveryPoint.RoomType.ToString(),
				Latitude = deliveryPoint.Latitude ?? 0,
				Longitude = deliveryPoint.Longitude ?? 0
			};
		}
	}
}
