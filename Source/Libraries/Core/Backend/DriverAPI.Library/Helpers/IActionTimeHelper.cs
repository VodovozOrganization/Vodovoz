using System;
using Vodovoz.Errors;

namespace DriverAPI.Library.Helpers
{
	public interface IActionTimeHelper
	{
		void ThrowIfNotValid(DateTime recievedTime, DateTime actionTime);
		Result CheckRequestTime(DateTime recievedTime, DateTime actionTime);
	}
}
