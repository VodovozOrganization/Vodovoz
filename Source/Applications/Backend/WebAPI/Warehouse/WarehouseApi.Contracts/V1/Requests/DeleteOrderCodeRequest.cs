using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.Contracts.V1.Requests
{
	/// <summary>
	/// DTO запроса удаления отсканированного кода номенклатуры в заказе
	/// </summary>
	public class DeleteOrderCodeRequest
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Id номенклатуры в заказе
		/// </summary>
		[Required]
		public int OrderSaleItemId { get; set; }

		/// <summary>
		/// Код для удаления в позиции из заказа
		/// </summary>
		[Required]
		public string DeletedCode { get; set; }
	}
}
