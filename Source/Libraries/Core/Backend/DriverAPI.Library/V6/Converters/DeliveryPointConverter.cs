using DriverApi.Contracts.V6;
using Gamma.Utilities;
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
		/// <param name="useRoomTypeDisplayName">Использовать отображаемое название типа помещения</param>
		/// <returns>Адрес точки доставки</returns>
		public AddressDto ExtractAPIAddressFromDeliveryPoint(DeliveryPoint deliveryPoint, bool useRoomTypeDisplayName = false)
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
				RoomType = useRoomTypeDisplayName ? deliveryPoint.RoomType.GetEnumTitle() : deliveryPoint.RoomType.ToString(),
				Latitude = deliveryPoint.Latitude ?? 0,
				Longitude = deliveryPoint.Longitude ?? 0
			};
		}
	}
}
