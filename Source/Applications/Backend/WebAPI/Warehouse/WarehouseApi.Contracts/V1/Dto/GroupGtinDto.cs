<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/GroupGtinDto.cs
﻿namespace WarehouseApi.Contracts.Dto.V1
========
﻿namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/GroupGtinDto.cs
{
	/// <summary>
	/// Информация о групповом Gtin
	/// </summary>
	public class GroupGtinDto
	{
		/// <summary>
		/// Номер товарной продукции GTIN (групповой)
		/// </summary>
		public string Gtin { get; set; }
		
		/// <summary>
		/// Количество товара в группе
		/// </summary>
		public int Count { get; set; }
	}
}
