using System;
using DriverAPI.Library.Deprecated.DTOs;

namespace DriverAPI.Library.Deprecated.Helpers
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public interface IActionTimeHelper
	{
		DateTime GetActionTime(IActionTimeTrackable actionTimeTrackable);
		void ThrowIfNotValid(DateTime recievedTime, DateTime actionTime);
	}
}
