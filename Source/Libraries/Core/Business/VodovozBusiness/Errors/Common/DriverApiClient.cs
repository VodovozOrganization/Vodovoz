namespace Vodovoz.Errors.Common
{
	public static class DriverApiClient
	{
		public static Error RequestIsNotSuccess(string message) =>
			new Error(typeof(DriverApiClient),
				nameof(RequestIsNotSuccess),
				$"Произошла ощибка при запросе к DriverAp: {message}");

		public static Error ApiError(string message) =>
			new Error(
				typeof(DriverApiClient),
				nameof(ApiError),
				message);

		public static Error OrderWithGoodsTransferingIsTransferedNotNotified(string message) =>
			new Error(
				typeof(DriverApiClient),
				nameof(OrderWithGoodsTransferingIsTransferedNotNotified),
				message);
	}
}
