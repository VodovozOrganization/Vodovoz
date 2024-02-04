using System;

namespace EventsApi.Library.Dtos
{
	public class CompletedDriverWarehouseEventDto
	{
		public string EventName { get; set; }
		public DateTime CompletedDate { get; set; }
		public string EmployeeName { get; set; }
	}
}
