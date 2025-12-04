<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Responses/V1/ChangeOrderCodeResponse.cs
﻿using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
========
﻿using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Responses/ChangeOrderCodeResponse.cs
{
	/// <summary>
	/// DTO ответа на запрос изменения кода маркировки ЧЗ для номенклатуры
	/// </summary>
	public class ChangeOrderCodeResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Номенклатура
		/// </summary>
		public NomenclatureDto Nomenclature { get; set; }
	}
}
