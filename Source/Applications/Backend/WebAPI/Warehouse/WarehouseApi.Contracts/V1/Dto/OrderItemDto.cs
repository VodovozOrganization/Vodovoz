using System.Collections.Generic;

<<<<<<<< HEAD:Source/Libraries/Core/Backend/WarehouseApi.Contracts/Dto/V1/OrderItemDto.cs
namespace WarehouseApi.Contracts.Dto.V1
========
namespace WarehouseApi.Contracts.V1.Dto
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi.Contracts/V1/Dto/OrderItemDto.cs
{
	/// <summary>
	/// Строка заказа
	/// </summary>
	public class OrderItemDto
	{
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Название номенклатуры
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Номера товарной продукции GTIN
		/// </summary>
		public IEnumerable<string> Gtin { get; set; }

		/// <summary>
		/// Номера группы товарной продукции GTIN
		/// </summary>
		public IEnumerable<GroupGtinDto> GroupGtins { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Коды маркировки ЧЗ
		/// </summary>
		public List<TrueMarkCodeDto> Codes { get; } = new List<TrueMarkCodeDto>();
	}
}
