using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReportViewModel
	{		
		public class SelectableParameter
		{
			public bool IsSelected { get; set; }
			public GeoGroup GeographicGroup { get; set; }
			public RouteListTypeOfUse RouteListTypeOfUse { get; set; }
		}
	}
}
