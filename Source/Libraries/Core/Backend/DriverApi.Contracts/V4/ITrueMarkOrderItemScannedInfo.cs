using System.Collections.Generic;

namespace DriverApi.Contracts.V4
{
	public interface ITrueMarkOrderItemScannedInfo
	{
		IEnumerable<string> BottleCodes { get; set; }
		IEnumerable<string> DefectiveBottleCodes { get; set; }
		int OrderSaleItemId { get; set; }
	}
}
