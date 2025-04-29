using System.Collections.Generic;

namespace DriverApi.Contracts.V6.Responses
{
	/// <summary>
	/// Переносы адресов водителя
	/// </summary>
	public class DriverTransfersInfoResponse
	{
		/// <summary>
		/// Входящие переносы
		/// </summary>
		public IEnumerable<RouteListAddressIncomingTransferInfo> IncomingTransfers { get; set; }

		/// <summary>
		/// Исходящие переносы
		/// </summary>
		public IEnumerable<RouteListAddressOutgoingTransferInfo> OutgoingTransfers { get; set; }
	}
}
