<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/CarLoadDocumentDto.cs
﻿namespace WarehouseApi.Contracts.Dto.V1
========
﻿namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/CarLoadDocumentDto.cs
{
	/// <summary>
	/// Документ погрузки авто
	/// </summary>
	public class CarLoadDocumentDto
	{
		/// <summary>
		/// Id талона погрузки авто
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Водитель
		/// </summary>
		public string Driver { get; set; }

		/// <summary>
		/// Авто
		/// </summary>
		public string Car { get; set; }

		/// <summary>
		/// Приоритет погрузки
		/// </summary>
		public int LoadPriority { get; set; }

		/// <summary>
		/// Состояние процесса погрузки номенклатур по документу
		/// </summary>
		public LoadOperationStateEnumDto State { get; set; }
	}
}
