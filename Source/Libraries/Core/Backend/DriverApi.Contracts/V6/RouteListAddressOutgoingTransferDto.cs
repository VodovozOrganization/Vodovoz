using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Информация о передаваемом адресе маршрутного листа
	/// </summary>
	public class RouteListAddressOutgoingTransferDto
	{
		/// <summary>
		/// Основная информация о переносе
		/// </summary>
		public RouteListAddressOutgoingTransferInfo RouteListAddressTransferInfo { get; set; }

		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<TransferItemDto> TransferItems { get; set; }

		/// <summary>
		/// Тип оплаты
		/// </summary>
		public PaymentDtoType PaymentType { get; set; }
	}
}
