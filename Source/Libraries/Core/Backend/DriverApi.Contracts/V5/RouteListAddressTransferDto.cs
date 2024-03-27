using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Информация о передаваемом/принимаемом заказе
	/// </summary>
	public class RouteListAddressTransferDto
	{
		/// <summary>
		/// Основная информация о переносе
		/// </summary>
		public RouteListAddressTransferInfo RouteListAddressTransferInfo { get; set; }

		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<TransferItemDto> TransferItems { get; set; }
	}
}
