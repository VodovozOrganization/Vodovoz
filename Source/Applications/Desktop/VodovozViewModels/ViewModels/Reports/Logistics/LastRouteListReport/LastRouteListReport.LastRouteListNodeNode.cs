using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport
{
	public partial class LastRouteListReport
	{
		public class LastRouteListNodeNode
		{
			public Employee Employee { get; set; }
			public int? LastRouteListId { get; set; }
		}
	}
}
