using System;

namespace DriverAPI.DTOs
{
	public class DelayedAction : IDelayedAction
	{
		public DateTime ActionTime { get; set; }
	}
}