using System.Collections.Generic;

namespace CustomerOrdersApi.Library.Default.Dto.Orders
{
	public class OrderRatingReasonDto
	{
		public int OrderRatingReasonId { get; set; }
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public IEnumerable<int> Ratings { get; set; }
	}
}
