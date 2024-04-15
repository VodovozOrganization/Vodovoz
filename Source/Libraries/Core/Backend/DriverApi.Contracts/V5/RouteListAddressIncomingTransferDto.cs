﻿using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Информация о принимаемом адресе маршрутного листа
	/// </summary>
	public class RouteListAddressIncomingTransferDto
	{
		/// <summary>
		/// Основная информация о переносе
		/// </summary>
		public RouteListAddressIncomingTransferInfo RouteListAddressTransferInfo { get; set; }

		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<TransferItemDto> TransferItems { get; set; }
	}
}
