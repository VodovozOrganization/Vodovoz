using System.Collections.Generic;

namespace WarehouseApi.Contracts.Dto
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
		/// Номер товарной продукции GTIN
		/// </summary>
		public IEnumerable<string> Gtin { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Коды маркировки ЧЗ
		/// </summary>
		public IEnumerable<TrueMarkCodeDto> Codes { get; set; }
	}
}
