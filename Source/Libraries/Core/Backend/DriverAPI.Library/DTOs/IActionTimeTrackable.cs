using System;

namespace DriverAPI.Library.DTOs
{
	public interface IActionTimeTrackable
	{
		DateTime? ActionTime { get; set; }
		DateTime? ActionTimeUtc { get; set; }
	}
}
