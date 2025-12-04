using System.Text.Json.Serialization;

<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/OperationResultEnumDto.cs
namespace WarehouseApi.Contracts.Dto.V1
========
namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/OperationResultEnumDto.cs
{
	/// <summary>
	/// Результат выполнения запроса
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OperationResultEnumDto
	{
		/// <summary>
		/// Успешно
		/// </summary>
		Success,
		/// <summary>
		/// Ошибка
		/// </summary>
		Error
	}
}
