using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
{
	/// <summary>
	/// DTO ответа на запрос удаления кода маркировки ЧЗ для номенклатуры
	/// </summary>
	public class DeleteOrderCodeResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Номенклатура
		/// </summary>
		public NomenclatureDto Nomenclature { get; set; }
	}
}
