using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
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
