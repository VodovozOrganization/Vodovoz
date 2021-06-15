using System;

namespace DriverAPI.Library.Models
{
	public class DriverActionDto
	{
		public ActionDtoType ActionType { get; set; }
		public DateTime ActionTime { get; set; }
	}
}
