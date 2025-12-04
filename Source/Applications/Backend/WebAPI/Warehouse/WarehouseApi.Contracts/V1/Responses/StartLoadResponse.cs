<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Responses/V1/StartLoadResponse.cs
﻿using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
========
﻿using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Responses/StartLoadResponse.cs
{
	/// <summary>
	/// DTO ответа на запрос начала погрузки талона
	/// </summary>
	public class StartLoadResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Документ погрузки
		/// </summary>
		public CarLoadDocumentDto CarLoadDocument { get; set; }
	}
}
