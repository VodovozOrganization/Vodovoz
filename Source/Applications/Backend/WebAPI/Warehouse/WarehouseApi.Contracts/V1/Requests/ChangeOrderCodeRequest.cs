using System.ComponentModel.DataAnnotations;

<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Requests/V1/ChangeOrderCodeRequest.cs
namespace WarehouseApi.Contracts.Requests.V1
========
namespace WarehouseApi.Contracts.V1.Requests
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Requests/ChangeOrderCodeRequest.cs
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
