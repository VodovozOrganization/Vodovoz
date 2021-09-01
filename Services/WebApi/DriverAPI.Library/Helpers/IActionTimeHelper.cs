using System;

namespace DriverAPI.Library.Helpers
{
	public interface IActionTimeHelper
	{
		void Validate(DateTime recievedTime, DateTime actionTime);
	}
}