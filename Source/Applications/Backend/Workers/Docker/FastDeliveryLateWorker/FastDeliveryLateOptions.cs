using System;

namespace FastDeliveryLateWorker
{
	public class FastDeliveryLateOptions
	{
		public TimeSpan Interval { get; set; }
		public int ComplaintDetalizationId { get; set; }
		public int ComplaintSourceId { get; set; }
		public int SouthGeoGroupId { get; set; }
		public int LoSofiyskayaSubdivisionId { get; set; }
		public int LoBugrySubdivisionId { get; set; }
		public int ResponsibleId { get; set; }
	}
}
