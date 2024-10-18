using Vodovoz.Errors;

namespace WarehouseApi.Library.Common
{
	public class RequestProcessingResult
	{
		protected internal RequestProcessingResult() { }

		public static RequestProcessingResult<TValue> CreateSuccess<TValue>(Result<TValue> result) =>
			new RequestProcessingResult<TValue>(result);

		public static RequestProcessingResult<TValue> CreateFailure<TValue>(Result<TValue> result, TValue failureValue) =>
			new RequestProcessingResult<TValue>(result, failureValue);
	}
}
