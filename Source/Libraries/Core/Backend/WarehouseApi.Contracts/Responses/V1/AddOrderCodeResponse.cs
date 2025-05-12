using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
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
