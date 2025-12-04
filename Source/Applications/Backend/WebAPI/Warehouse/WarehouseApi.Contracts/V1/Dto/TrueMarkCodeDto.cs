<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/TrueMarkCodeDto.cs
﻿namespace WarehouseApi.Contracts.Dto.V1
========
﻿namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/TrueMarkCodeDto.cs
{
	/// <summary>
	/// Код маркировки ЧЗ
	/// </summary>
	public class TrueMarkCodeDto
	{
		/// <summary>
		/// Порядковый номер
		/// </summary>
		public int SequenceNumber { get; set; }

		/// <summary>
		/// Код честного знака
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Уровень кода
		/// </summary>
		public WarehouseApiTruemarkCodeLevel Level { get; set; }

		/// <summary>
		/// Родительский код
		/// </summary>
		public string Parent { get; set; }
	}
}
