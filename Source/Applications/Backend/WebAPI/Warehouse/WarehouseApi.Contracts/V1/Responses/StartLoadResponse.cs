using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
{
	/// <summary>
	/// DTO ответа на запрос начала погрузки талона
	/// </summary>
	public class StartLoadResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Документ погрузки
		/// </summary>
		public CarLoadDocumentDto CarLoadDocument { get; set; }
	}
}
