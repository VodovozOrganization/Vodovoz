using System.ComponentModel.DataAnnotations;

<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Requests/V1/AddOrderCodeRequest.cs
namespace WarehouseApi.Contracts.Requests.V1
========
namespace WarehouseApi.Contracts.V1.Requests
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Requests/AddOrderCodeRequest.cs
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
