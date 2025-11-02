using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.Contracts.V1.Requests
{
	/// <summary>
	/// DTO запроса замены отсканированного кода номенклатуры в заказе
	/// </summary>
	public class ChangeOrderCodeRequest
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Id номенклатуры
		/// </summary>
		[Required]
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Старый код маркировки ЧЗ
		/// </summary>
		[Required]
		public string OldCode { get; set; }

		/// <summary>
		/// Новый код маркировки ЧЗ
		/// </summary>
		public string NewCode { get; set; }
	}
}
