using System;

namespace DriverAPI.Library.Helpers
{
	public interface IActionTimeHelper
	{
		void ThrowIfNotValid(DateTime recievedTime, DateTime actionTime);
	}
}
