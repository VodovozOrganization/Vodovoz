﻿using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
	/// <summary>
	/// DTO ответа на запрос получения информации о заказе
	/// </summary>
	public class GetOrderResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Данные по заказу
		/// </summary>
		public OrderDto Order { get; set; }
	}
}
