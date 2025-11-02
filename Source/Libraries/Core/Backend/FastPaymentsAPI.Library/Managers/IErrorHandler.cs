using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IErrorHandler
	{
		TResponse LogAndReturnErrorMessageFromRegistrationOrder<TResponse>(
			TResponse errorResponse, IAvangardResponseDetails bankResponseDetails, int orderId, bool isOnlineOrder, ILogger logger)
			where TResponse : class, IErrorResponse;
		TResponse LogAndReturnErrorMessageFromUpdateOrderInfo<TResponse>(
			TResponse errorResponse, IAvangardResponseDetails bankResponseDetails, string ticket, ILogger logger)
			where TResponse : class, IErrorResponse;
		void LogErrorMessageFromUpdateOrderInfo(IAvangardResponseDetails bankResponseDetails, string ticket, ILogger logger);
	}
}
