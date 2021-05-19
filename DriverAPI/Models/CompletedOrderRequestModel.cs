using System;

namespace DriverAPI.Models
{
	public class CompletedOrderRequestModel
	{
		public int OrderId { get; set; }
		public int BottlesReturnCount { get; set; }
		public int Rating { get; set; }
		public int DriverComplaintReasonId { get; set; }
		public string OtherDriverComplaintReasonComment { get; set; }
		public DateTime ActionTime { get; set; }
	}
}
