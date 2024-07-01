using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace FastPaymentsAPI.Library.Managers
{
	public class ErrorHandler : IErrorHandler
	{
		public TResponse LogAndReturnErrorMessageFromRegistrationOrder<TResponse>(
			TResponse errorResponse, IAvangardResponseDetails bankResponseDetails, int orderId, bool isOnlineOrder, ILogger logger)
			where TResponse : class, IErrorResponse
		{
			var message = GetRegistrationOrderErrorMessage(orderId, isOnlineOrder);
			errorResponse.ErrorMessage = message;
			logger.LogError(message + " Код ответа {ResponseCode}\n{ResponseMessage}",
				bankResponseDetails.ResponseCode,
				bankResponseDetails.ResponseMessage);
			
			return errorResponse;
		}
		
		public void LogErrorMessageFromUpdateOrderInfo(IAvangardResponseDetails bankResponseDetails, string ticket, ILogger logger)
		{
			var message = GetUpdateOrderInfoErrorMessage(ticket);
			logger.LogError(message + " Код ответа {ResponseCode}\n{ResponseMessage}",
				bankResponseDetails.ResponseCode,
				bankResponseDetails.ResponseMessage);
		}
		
		public TResponse LogAndReturnErrorMessageFromUpdateOrderInfo<TResponse>(
			TResponse errorResponse, IAvangardResponseDetails bankResponseDetails, string ticket, ILogger logger)
			where TResponse : class, IErrorResponse
		{
			var message = GetUpdateOrderInfoErrorMessage(ticket);
			errorResponse.ErrorMessage = message;
			logger.LogError(message + " Код ответа {ResponseCode}\n{ResponseMessage}",
				bankResponseDetails.ResponseCode,
				bankResponseDetails.ResponseMessage);
			
			return errorResponse;
		}
		
		private static string GetRegistrationOrderErrorMessage(int orderId, bool isOnlineOrder) =>
			$"При регистрации {GetOrderText(isOnlineOrder)} {orderId} для отправки QR-кода в системе эквайринга произошла ошибка";
		
		private static string GetUpdateOrderInfoErrorMessage(string ticket) =>
			$"При получении инфы о сессии {ticket} произошла ошибка";

		private static string GetOrderText(bool isOnlineOrder) => isOnlineOrder ? "онлайн-заказа" : "заказа";
	}
}
