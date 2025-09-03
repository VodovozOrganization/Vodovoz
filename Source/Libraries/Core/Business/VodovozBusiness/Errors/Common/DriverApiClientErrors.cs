using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Common
{
	public static class DriverApiClientErrors
	{
		public static Error RequestIsNotSuccess(string message) =>
			new Error(typeof(DriverApiClientErrors),
				nameof(RequestIsNotSuccess),
				$"Произошла ощибка при запросе к DriverAp: {message}");

		public static Error ApiError(string message) =>
			new Error(
				typeof(DriverApiClientErrors),
				nameof(ApiError),
				message);

		public static Error OrderWithGoodsTransferingIsTransferedNotNotified(string message) =>
			new Error(
				typeof(DriverApiClientErrors),
				nameof(OrderWithGoodsTransferingIsTransferedNotNotified),
				message);
	}
}
