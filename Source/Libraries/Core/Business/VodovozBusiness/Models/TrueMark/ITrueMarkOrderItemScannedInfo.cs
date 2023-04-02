using System.Collections.Generic;

namespace Vodovoz.Models.TrueMark
{
	public interface ITrueMarkOrderItemScannedInfo
	{
		IEnumerable<string> BottleCodes { get; set; }
		IEnumerable<string> DefectiveBottleCodes { get; set; }
		int OrderSaleItemId { get; set; }
	}
}
