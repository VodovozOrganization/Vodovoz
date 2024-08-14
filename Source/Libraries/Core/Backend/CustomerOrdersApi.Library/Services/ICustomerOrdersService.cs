using System.Collections.Generic;
using CustomerOrdersApi.Library.Dto.Orders;

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
	}
}
