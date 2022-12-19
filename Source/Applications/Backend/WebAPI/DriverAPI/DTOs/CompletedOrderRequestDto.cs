using System;
using System.Collections.Generic;

namespace DriverAPI.DTOs
{
	public class CompletedOrderRequestDto
	{
		public int OrderId { get; set; }
		public int BottlesReturnCount { get; set; }
		public int Rating { get; set; }
		public int DriverComplaintReasonId { get; set; }
		public string OtherDriverComplaintReasonComment { get; set; }
		public DateTime ActionTime { get; set; }
		public string DriverComment { get; set; }
		public IEnumerable<OrderScannedItemDto> ScannedBottles { get; set; }
		public string UnscannedBottlesReason { get; set; }
	}
}
