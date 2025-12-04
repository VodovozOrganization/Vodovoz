using System.Text.Json.Serialization;

<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/WarehouseApiTruemarkCodeLevel.cs
namespace WarehouseApi.Contracts.Dto.V1
========
namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/WarehouseApiTruemarkCodeLevel.cs
{
	/// <summary>
	/// Уровень кода маркировки ЧЗ
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum WarehouseApiTruemarkCodeLevel
	{
		/// <summary>
		/// Транспортный код
		/// </summary>
		transport,
		/// <summary>
		/// Групповой код
		/// </summary>
		group,
		/// <summary>
		/// Единичный код
		/// </summary>
		unit
	}
}
