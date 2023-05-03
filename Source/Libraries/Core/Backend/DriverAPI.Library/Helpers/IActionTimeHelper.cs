using System;
using DriverAPI.Library.Deprecated.DTOs;

namespace DriverAPI.Library.Helpers
{
	public interface IActionTimeHelper
	{
		DateTime GetActionTime(IActionTimeTrackable actionTimeTrackable);
		void ThrowIfNotValid(DateTime recievedTime, DateTime actionTime);
	}
}
