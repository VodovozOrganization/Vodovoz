using System;

namespace DriverAPI.Library.Deprecated.DTOs
{
	public interface IActionTimeTrackable
	{
		DateTime? ActionTime { get; set; }
		DateTime? ActionTimeUtc { get; set; }
	}
}
