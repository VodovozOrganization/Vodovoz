using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses
{
	/// <summary>
	/// DTO ответа на запрос добавления кода маркировки ЧЗ в заказ
	/// </summary>
	public class AddOrderCodeResponse : ResponseBase
	{
		/// <summary>
		/// Номенклатура
		/// </summary>
		public NomenclatureDto Nomenclature { get; set; }
	}
}
