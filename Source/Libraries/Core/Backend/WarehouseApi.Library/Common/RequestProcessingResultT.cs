using System;
using Vodovoz.Errors;

namespace WarehouseApi.Library.Common
{
	public class RequestProcessingResult<TValue> : RequestProcessingResult
	{
		private readonly Result<TValue> _result;
		private readonly TValue _failureData;

		protected internal RequestProcessingResult(Result<TValue> result)
		{
			_result = result ?? throw new ArgumentNullException(nameof(result));
		}

		protected internal RequestProcessingResult(Result<TValue> result, TValue failureData)
		{
			_result = result ?? throw new ArgumentNullException(nameof(result));
			_failureData = failureData;
		}

		public Result<TValue> Result => _result;

		public TValue FailureData =>
			(Result is null || Result.IsSuccess)
			? throw new InvalidOperationException("The failure data can't be accessed")
			: _failureData;
	}
}
