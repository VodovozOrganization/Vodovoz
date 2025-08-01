using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.Contracts.V1.Requests
{
	/// <summary>
	/// DTO запроса добавления кода маркировки ЧЗ
	/// </summary>
	public class AddOrderCodeRequest
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
		/// Код ЧЗ
		/// </summary>
		[Required]
		public string Code { get; set; }
	}
}
