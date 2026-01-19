using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.Contracts.V1.Requests
{
	/// <summary>
	/// DTO запроса установки склада по умолчанию
	/// </summary>
	public class ChangeDefaultWarehouseRequest
	{
		/// <summary>
		/// Id склада
		/// </summary>
		[Required]
		public int WarehouseId { get; set; }
	}
}
