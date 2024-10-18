using Vodovoz.Errors;

namespace VodovozBusiness.Errors.Common
{
	public static class WarehouseApiClient
	{
		public static Error RequestIsNotSuccess(string message) =>
			new Error(typeof(WarehouseApiClient),
				nameof(RequestIsNotSuccess),
				$"Произошла ошибка при запросе к WarehouseApi: {message}");
	}
}
