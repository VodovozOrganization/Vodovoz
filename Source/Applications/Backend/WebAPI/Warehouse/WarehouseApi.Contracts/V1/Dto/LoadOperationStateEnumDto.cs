using System.Text.Json.Serialization;

<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/LoadOperationStateEnumDto.cs
namespace WarehouseApi.Contracts.Dto.V1
========
namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/LoadOperationStateEnumDto.cs
{
	/// <summary>
	/// Статус выполнения операции погрузки
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum LoadOperationStateEnumDto
	{
		/// <summary>
		/// Погрузка не начата
		/// </summary>
		NotStarted,
		/// <summary>
		/// В процессе погрузки
		/// </summary>
		InProgress,
		/// <summary>
		/// Погрузка завершена
		/// </summary>
		Done
	}
}
