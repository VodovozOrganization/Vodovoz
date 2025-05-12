using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
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
