using System.Text.Json.Serialization;
using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
	public class ResponseBase
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
