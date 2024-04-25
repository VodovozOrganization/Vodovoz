﻿using System;

namespace DriverApi.Contracts.V5.Requests
{
	/// <summary>
	/// Запрос на подтверждение передачи переноса адреса маршрутного листа
	/// </summary>
	public class ConfirmRouteListAddressTransferTransferedRequest
	{
		/// <summary>
		/// Идентификатор адреса маршрутного листа
		/// </summary>
		public int RouteListAddress { get; set; }

		/// <summary>
		/// Время нажатия на кнопку подтверждения в мобильном приложении водителей
		/// </summary>
		public DateTime ActionTime { get; set; }
	}
}
