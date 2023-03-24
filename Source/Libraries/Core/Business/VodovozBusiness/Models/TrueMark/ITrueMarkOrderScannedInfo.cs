using System.Collections.Generic;

namespace Vodovoz.Models.TrueMark
{
	public interface ITrueMarkOrderScannedInfo
	{
		IEnumerable<ITrueMarkOrderItemScannedInfo> ScannedItems { get; }
		string UnscannedCodesReason { get; }
	}
}
