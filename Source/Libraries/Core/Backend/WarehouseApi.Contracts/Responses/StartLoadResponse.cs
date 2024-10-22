using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
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
