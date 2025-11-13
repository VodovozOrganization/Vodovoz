using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
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
