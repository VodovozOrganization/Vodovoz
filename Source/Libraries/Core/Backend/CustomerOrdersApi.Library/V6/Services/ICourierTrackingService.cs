using CustomerOrdersApi.Library.V6.Dto.Orders;
using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders.V6;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Services
{
	/// <summary>
	/// Сервис для работы с отслеживанием координат курьера
	/// </summary>
	public partial interface ICourierTrackingService
	{
		/// <summary>
		/// Получение координат курьера для заказа
		/// </summary>
		/// <param name="getCourierCoordinatesDto">Данные по заказу</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные координат курьера</returns>
		Task<Result<CourierCoordinatesDto>> GetCourierCoordinates(GetCourierCoordinatesDto getCourierCoordinatesDto, CancellationToken cancellationToken = default);

		/// <summary>
		/// Получение координат курьера для заказа
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="order">Заказ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные о координатах курьера</returns>
		Task<DriverPositionData> GetDriverPositionData(IUnitOfWork uow, Order order, CancellationToken cancellationToken = default);
		
		/// <summary>
		/// Получение координат курьера для заказа
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="order">Заказ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные о координатах курьера</returns>
		Task<DriverPositionData> GetDriverPositionData(IUnitOfWork uow, OrderDto order, CancellationToken cancellationToken = default);
	}
}
