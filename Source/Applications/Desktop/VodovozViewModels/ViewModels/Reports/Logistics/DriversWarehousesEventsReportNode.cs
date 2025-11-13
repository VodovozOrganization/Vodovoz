using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics
{
	public class DriversWarehousesEventsReportNode
	{
		public DateTime EventDate { get; set; }
    public string DriverFio { get; set; }
    public string CarModelWithNumber { get; set; }
    
    // Первое событие
    public int FirstEventId { get; set; }
    public string FirstEventName { get; set; }
    public string FirstEventDocumentType { get; set; }
    public int? FirstEventDocumentNumber { get; set; }
    public decimal FirstEventDistance { get; set; }
    
    public TimeSpan? FirstEventTime { get; set; }
    
    // Второе событие
    public int? SecondEventId { get; set; }
    public string SecondEventName { get; set; }
    public string SecondEventDocumentType { get; set; }
    public int? SecondEventDocumentNumber { get; set; }
    public decimal? SecondEventDistance { get; set; }
    public TimeSpan? SecondEventTime { get; set; }
	}
}
