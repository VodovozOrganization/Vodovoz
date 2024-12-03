using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
	/// <summary>
	/// DTO ответа на запрос добавления кода маркировки ЧЗ в заказ
	/// </summary>
	public class AddOrderCodeResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Номенклатура
		/// </summary>
		public NomenclatureDto Nomenclature { get; set; }
	}
}
