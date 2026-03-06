using CustomerOrdersApi.Library.Dto.Orders;
using System.Collections.Generic;

namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Сервис для работы с онлайн заказами из внешних источников
	/// </summary>
	public interface ICustomerOrdersService
	{
		/// <summary>
		/// Валидирует подпись заказа для проверки подлинности источника
		/// </summary>
		/// <param name="onlineOrderInfoDto">DTO с информацией о заказе</param>
		/// <param name="generatedSignature">Сгенерированная подпись для сравнения</param>
		/// <returns>true, если подпись валидна; иначе false</returns>
		bool ValidateOrderSignature(OnlineOrderInfoDto onlineOrderInfoDto, out string generatedSignature);

		/// <summary>
		/// Валидирует подпись оценки заказа для проверки подлинности источника
		/// </summary>
		/// <param name="orderRatingInfo">DTO с информацией об оценке заказа</param>
		/// <param name="generatedSignature">Сгенерированная подпись для сравнения</param>
		/// <returns>true, если подпись валидна; иначе false</returns>
		bool ValidateOrderRatingSignature(OrderRatingInfoForCreateDto orderRatingInfo, out string generatedSignature);

		/// <summary>
		/// Валидирует подпись запроса на получение информации о заказе
		/// </summary>
		/// <param name="getDetailedOrderInfoDto">DTO с запросом на получение деталей заказа</param>
		/// <param name="generatedSignature">Сгенерированная подпись для сравнения</param>
		/// <returns>true, если подпись валидна; иначе false</returns>
		bool ValidateOrderInfoSignature(GetDetailedOrderInfoDto getDetailedOrderInfoDto, out string generatedSignature);

		/// <summary>
		/// Валидирует подпись запроса на получение заказов контрагента
		/// </summary>
		/// <param name="getOrdersDto">DTO с запросом на получение заказов</param>
		/// <param name="generatedSignature">Сгенерированная подпись для сравнения</param>
		/// <returns>true, если подпись валидна; иначе false</returns>
		bool ValidateCounterpartyOrdersSignature(GetOrdersDto getOrdersDto, out string generatedSignature);

		/// <summary>
		/// Валидирует подпись запроса на создание заявки на обратный звонок
		/// </summary>
		/// <param name="creatingInfoDto">DTO с информацией для создания заявки на звонок</param>
		/// <param name="generatedSignature">Сгенерированная подпись для сравнения</param>
		/// <returns>true, если подпись валидна; иначе false</returns>
		bool ValidateRequestForCallSignature(CreatingRequestForCallDto creatingInfoDto, out string generatedSignature);

		/// <summary>
		/// Получает детальную информацию о заказе
		/// </summary>
		/// <param name="getDetailedOrderInfoDto">DTO с параметрами запроса на получение деталей</param>
		/// <returns>DTO с детальной информацией о заказе</returns>
		DetailedOrderInfoDto GetDetailedOrderInfo(GetDetailedOrderInfoDto getDetailedOrderInfoDto);

		/// <summary>
		/// Получает список заказов контрагента с постраничной навигацией
		/// </summary>
		/// <param name="getOrdersDto">DTO с параметрами запроса на получение заказов</param>
		/// <returns>DTO с информацией о заказах</returns>
		OrdersDto GetOrders(GetOrdersDto getOrdersDto);

		/// <summary>
		/// Получает список причин для оценки заказов
		/// </summary>
		/// <returns>Перечисление DTO с причинами оценки</returns>
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasons();

		/// <summary>
		/// Создает оценку для заказа
		/// </summary>
		/// <param name="orderRatingInfo">DTO с информацией об оценке заказа</param>
		void CreateOrderRating(OrderRatingInfoForCreateDto orderRatingInfo);

		/// <summary>
		/// Пытается обновить статус оплаты онлайн заказа
		/// </summary>
		/// <param name="paymentStatusUpdatedDto">DTO с новым статусом оплаты</param>
		/// <returns>true, если статус успешно обновлен; иначе false</returns>
		bool TryUpdateOnlineOrderPaymentStatus(OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto);

		/// <summary>
		/// Создает заявку на обратный звонок от клиента
		/// </summary>
		/// <param name="creatingInfoDto">DTO с информацией для создания заявки на звонок</param>
		void CreateRequestForCall(CreatingRequestForCallDto creatingInfoDto);

		/// <summary>
		/// Проверяет возможность отмены заказа на основе его текущего статуса и других условий
		/// </summary>
		/// <param name="cancelOrderDto">DTO с данными заказа для отмены</param>
		/// <returns>DTO с результатом проверки возможности отмены, включая информацию о необходимости контакта с менеджером</returns>
		CancellationCheckResultDto CanCancelOrder(CancelOrderDto cancelOrderDto);
	}
}
