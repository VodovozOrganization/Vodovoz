using CustomerOrdersApi.Library.Dto.Orders;

namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Интерфейс сервиса по работе с отображением курьера на карте
	/// </summary>
	public interface ICustomerOrdersTrackingCourierService
	{
		/// <summary>
		/// Получение текущих координат курьера(водителя) по онлайн заказу
		/// </summary>
		/// <param name="coordinatesRequest">Данные запроса <see cref="CourierCoordinatesRequest"/></param>
		/// <returns>Данные с координатами курьера и адреса <see cref="CourierCoordinates"/></returns>
		CourierCoordinates GetCurrentCourierCoordinates(CourierCoordinatesRequest coordinatesRequest);
		/// <summary>
		/// Проверка валидности источника запроса
		/// </summary>
		/// <param name="coordinatesRequest">Данные запроса <see cref="CourierCoordinatesRequest"/></param>
		/// <param name="generatedSignature">Сгенерированная подпись</param>
		/// <returns><c>true</c> - подпись валидна, <c>false</c> - подпись не валидна</returns>
		bool ValidateCourierCoordinatesSignature(CourierCoordinatesRequest coordinatesRequest, out string generatedSignature);
	}
}
