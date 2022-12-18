using System.Collections.Generic;

namespace DriverAPI.DTOs
{
	public class OrderScannedItemDto
	{
		public int OrderSaleItemId { get; set; }
		public IEnumerable<string> BottleCodes { get; set; }
		public IEnumerable<string> DefectiveBottleCodes { get; set; }
	}
}
