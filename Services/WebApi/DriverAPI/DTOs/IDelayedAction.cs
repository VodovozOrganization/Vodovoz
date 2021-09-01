using System;

namespace DriverAPI.DTOs
{
	public interface IDelayedAction
	{
		DateTime ActionTime { get; set; }
	}
}