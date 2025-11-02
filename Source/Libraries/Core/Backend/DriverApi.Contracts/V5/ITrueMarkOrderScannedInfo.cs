using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	public interface ITrueMarkOrderScannedInfo
	{
		IEnumerable<ITrueMarkOrderItemScannedInfo> ScannedItems { get; }
		string UnscannedCodesReason { get; }
	}
}
