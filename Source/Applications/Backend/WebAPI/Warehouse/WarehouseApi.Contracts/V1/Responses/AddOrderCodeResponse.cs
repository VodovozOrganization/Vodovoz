using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Contracts.V1.Responses
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
