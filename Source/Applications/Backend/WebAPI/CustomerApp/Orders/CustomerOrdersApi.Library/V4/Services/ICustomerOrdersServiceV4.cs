using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V4.Services
{
	public interface ICustomerOrdersServiceV4
	{
		/// <summary>
		/// Проверка контрольной суммы запроса создания заказа
		/// </summary>
		/// <param name="creatingOnlineOrder">Информация об онлайн заказе с ИПЗ</param>
		/// <param name="generatedSignature">Сгенерированная контрольная сумма Erp, для проверки</param>
		/// <returns><c>true</c> - валидный запрос, <c>false</c> - невалидный запрос</returns>
		bool ValidateOrderSignature(ICreatingOnlineOrder creatingOnlineOrder, out string generatedSignature);
		/// <summary>
		/// Проверка контрольной суммы запроса оценки заказа
		/// </summary>
		/// <param name="orderRatingInfo">Информация об оценке заказа с ИПЗ</param>
		/// <param name="generatedSignature">Сгенерированная контрольная сумма Erp, для проверки</param>
		/// <returns><c>true</c> - валидный запрос, <c>false</c> - невалидный запрос</returns>
		bool ValidateOrderRatingSignature(OrderRatingInfoForCreateDto orderRatingInfo, out string generatedSignature);
		/// <summary>
		/// Проверка контрольной суммы запроса получения деталей заказа
		/// </summary>
		/// <param name="getDetailedOrderInfoDto">Информация для получения деталей заказа</param>
		/// <param name="generatedSignature">Сгенерированная контрольная сумма Erp, для проверки</param>
		/// <returns><c>true</c> - валидный запрос, <c>false</c> - невалидный запрос</returns>
		bool ValidateOrderInfoSignature(GetDetailedOrderInfoDto getDetailedOrderInfoDto, out string generatedSignature);
		/// <summary>
		/// Проверка контрольной суммы запроса заказов клиента
		/// </summary>
		/// <param name="getOrdersDto">Информация для получения заказов клиента</param>
		/// <param name="generatedSignature">Сгенерированная контрольная сумма Erp, для проверки</param>
		/// <returns><c>true</c> - валидный запрос, <c>false</c> - невалидный запрос</returns>
		bool ValidateCounterpartyOrdersSignature(GetOrdersDto getOrdersDto, out string generatedSignature);
		/// <summary>
		/// Проверка контрольной суммы запроса заявки на звонок
		/// </summary>
		/// <param name="creatingInfoDto">Информация для создания заявки на звонок</param>
		/// <param name="generatedSignature">Сгенерированная контрольная сумма Erp, для проверки</param>
		/// <returns><c>true</c> - валидный запрос, <c>false</c> - невалидный запрос</returns>
		bool ValidateRequestForCallSignature(CreatingRequestForCallDto creatingInfoDto, out string generatedSignature);
		/// <summary>
		/// Получение деталей заказа
		/// </summary>
		/// <param name="getDetailedOrderInfoDto">Данные для получения деталей заказа</param>
		/// <returns>Детали заказа <see cref="DetailedOrderInfoDto"/></returns>
		DetailedOrderInfoDto GetDetailedOrderInfo(GetDetailedOrderInfoDto getDetailedOrderInfoDto);
		/// <summary>
		/// Получение заказов клиента
		/// </summary>
		/// <param name="getOrdersDto">Данные для получения заказов клиента</param>
		/// <returns>Заказы клиента <see cref="GetOrdersDto"/></returns>
		OrdersDto GetOrders(GetOrdersDto getOrdersDto);
		/// <summary>
		/// Получение списка причин оценки заказа
		/// </summary>
		/// <returns>Список причин оценки заказа <see cref="OrderRatingReasonDto"/></returns>
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasons();
		/// <summary>
		/// Оценка заказа
		/// </summary>
		/// <param name="orderRatingInfo">Данные по оценке заказа</param>
		void CreateOrderRating(OrderRatingInfoForCreateDto orderRatingInfo);
		/// <summary>
		/// Создание заявки на звонок
		/// </summary>
		/// <param name="creatingInfoDto">Данные по заявке на звонок</param>
		void CreateRequestForCall(CreatingRequestForCallDto creatingInfoDto);
		/// <summary>
		/// Получение доступных форм оплат
		/// </summary>
		/// <param name="getAvailablePaymentMethods">Данные для получения доступных способов оплат</param>
		/// <returns></returns>
		(int HttpCode, string Message, AvailablePaymentMethods AvailablePayments) GetAvailablePaymentMethods(
			GetAvailablePaymentMethodsDto getAvailablePaymentMethods);
		/// <summary>
		/// Обновление заказа
		/// </summary>
		/// <param name="changingOrderDto">Данные по обновляемому заказу</param>
		/// <param name="cancellationToken">Токен для отмены операции</param>
		/// <returns></returns>
		Task<Result<ChangedOrderDto>> UpdateOrderAsync(ChangingOrderDto changingOrderDto, CancellationToken cancellationToken);
	}
}
