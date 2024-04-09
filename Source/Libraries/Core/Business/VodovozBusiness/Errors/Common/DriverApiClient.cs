using Vodovoz.Errors;

namespace Vodovoz.Errors.Common
{
	public static class DriverApiClient
	{
		public static Error RequestIsNotSuccess(string message) =>
			new Error(typeof(DriverApiClient),
				nameof(RequestIsNotSuccess),
				$"Произошла ощибка при запросе к DriverAp: {message}");
	}
}
