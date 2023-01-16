using System.Collections.Generic;
using Vodovoz.Models.TrueMark;

namespace DriverAPI.DTOs
{

	public class OrderScannedItemDto : ITrueMarkOrderItemScannedInfo
	{
		public int OrderSaleItemId { get; set; }
		public IEnumerable<string> BottleCodes { get; set; }
		public IEnumerable<string> DefectiveBottleCodes { get; set; }
	}
}
