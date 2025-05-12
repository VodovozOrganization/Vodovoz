using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
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
