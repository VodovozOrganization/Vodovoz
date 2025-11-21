using CustomerOrdersApi.Library.Dto.Orders;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Results;

namespace CustomerOrdersApi.Library.Services
{
	public interface ICustomerOrdersService
	{
		bool ValidateOrderSignature(OnlineOrderInfoDto onlineOrderInfoDto, out string generatedSignature);
		bool ValidateOrderRatingSignature(OrderRatingInfoForCreateDto orderRatingInfo, out string generatedSignature);
		bool ValidateOrderInfoSignature(GetDetailedOrderInfoDto getDetailedOrderInfoDto, out string generatedSignature);
		bool ValidateCounterpartyOrdersSignature(GetOrdersDto getOrdersDto, out string generatedSignature);
		bool ValidateRequestForCallSignature(CreatingRequestForCallDto creatingInfoDto, out string generatedSignature);
		DetailedOrderInfoDto GetDetailedOrderInfo(GetDetailedOrderInfoDto getDetailedOrderInfoDto);
		OrdersDto GetOrders(GetOrdersDto getOrdersDto);
		IEnumerable<OrderRatingReasonDto> GetOrderRatingReasons();
		void CreateOrderRating(OrderRatingInfoForCreateDto orderRatingInfo);
		bool TryUpdateOnlineOrderPaymentStatus(OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto);
		void CreateRequestForCall(CreatingRequestForCallDto creatingInfoDto);

		/// <summary>
		/// Валидация контрольной суммы, для проверки валидности отправителя
		/// </summary>
		/// <param name="getRecomendationsDto">Данные запроса на получение рекомендаций к заказу</param>
		/// <param name="generatedSignature">Сгенерированная контрольная сумма</param>
		/// <returns>Результат проверки контрольной суммы</returns>
		bool ValidateRequestRecomendationsSignature(GetRecomendationsDto getRecomendationsDto, out string generatedSignature);

		/// <summary>
		/// Получение рекомендаций к заказу
		/// </summary>
		/// <param name="getRecomendationsDto">Данные запроса на получение рекомендаций к заказу</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список строк рекомендаций</returns>
		Task<Result<IEnumerable<RecomendationItemDto>, Exception>> GetRecomendations(GetRecomendationsDto getRecomendationsDto, CancellationToken cancellationToken = default);
	}
}
