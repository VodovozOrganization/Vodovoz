using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics
{
	public class DriversWarehousesEventsReportNode
	{
		public DateTime EventDate { get; set; }
		public string DriverFio { get; set; }
		public string CarModelWithNumber { get; set; }
		public string FirstEventName { get; set; }
		public decimal? FirstEventDistance { get; set; }
		public TimeSpan? FirstEventTime { get; set; }
		public string SecondEventName { get; set; }
		public decimal? SecondEventDistance { get; set; }
		public TimeSpan? SecondEventTime { get; set; }
	}
}
