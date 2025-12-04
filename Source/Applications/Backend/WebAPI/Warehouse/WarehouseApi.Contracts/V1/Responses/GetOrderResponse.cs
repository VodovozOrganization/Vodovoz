<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Responses/V1/GetOrderResponse.cs
﻿using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
========
﻿using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Responses/GetOrderResponse.cs
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
