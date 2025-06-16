﻿using CustomerOrdersApi.Library.Dto.Orders;
using System;
using System.Collections.Generic;
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
		bool ValidateRequestRecomendationsSignature(GetRecomendationsDto getRecomendationsDto, out string generatedSignature);
		Result<IEnumerable<RecomendationItemDto>, Exception> GetRecomendations(GetRecomendationsDto getRecomendationsDto);
	}
}
