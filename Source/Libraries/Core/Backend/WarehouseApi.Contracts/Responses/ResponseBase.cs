using System.Text.Json.Serialization;

namespace WarehouseApi.Contracts.Responses
{
	public class ResponseBase
	{
		/// <summary>
		/// Результат выполнения операции
		/// </summary>
		//[JsonPropertyName("result")]
		public OperationResult Result { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		//[JsonPropertyName("error")]
		public string Error { get; set; }
	}
}
