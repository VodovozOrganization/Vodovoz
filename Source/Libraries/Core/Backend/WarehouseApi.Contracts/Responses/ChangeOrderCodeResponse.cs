using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
	/// <summary>
	/// DTO ответа на запрос изменения кода маркировки ЧЗ для номенклатуры
	/// </summary>
	public class ChangeOrderCodeResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Номенклатура
		/// </summary>
		public NomenclatureDto Nomenclature { get; set; }
	}
}
