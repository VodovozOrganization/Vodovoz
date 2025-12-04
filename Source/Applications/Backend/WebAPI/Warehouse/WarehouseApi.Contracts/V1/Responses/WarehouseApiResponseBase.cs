<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Responses/V1/WarehouseApiResponseBase.cs
﻿using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
========
﻿using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Responses/WarehouseApiResponseBase.cs
{
	/// <summary>
	/// Базовый класс DTO ответа
	/// </summary>
	public class WarehouseApiResponseBase
	{
		/// <summary>
		/// Результат выполнения операции
		/// </summary>
		public OperationResultEnumDto Result { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string Error { get; set; }
	}
}
