using System.Text.Json.Serialization;
using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
	public class StartLoadResponse :ResponseBase
	{
		/// <summary>
		/// Документ погрузки
		/// </summary>
		[JsonPropertyName("carLoadDocument")]
		public CarLoadDocumentDto CarLoadDocument { get; set; }
	}
}
