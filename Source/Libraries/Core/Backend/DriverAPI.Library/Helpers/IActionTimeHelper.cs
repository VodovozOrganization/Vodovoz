using System;
using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Helpers
{
	public interface IActionTimeHelper
	{
		void ThrowIfNotValid(DateTime recievedTime, DateTime actionTime);
		Result CheckRequestTime(DateTime recievedTime, DateTime actionTime);
	}
}
