using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Данные по номенклатуре и кодам маркировки ЧЗ строки заказа
	/// </summary>
	public class NomenclatureTrueMarkCodesDto
	{
		/// <summary>
		/// Номер товара на продажу
		/// </summary>
		public int OrderSaleItemId { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// GTIN товара
		/// </summary>
		public IEnumerable<string> Gtin { get; set; }

		/// <summary>
		/// Номера группы товарной продукции GTIN
		/// </summary>
		public IEnumerable<GroupGtinDto> GroupGtins { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public decimal Quantity { get; set; }

		/// <summary>
		/// Коды маркировки ЧЗ
		/// </summary>
		public IEnumerable<TrueMarkCodeDto> Codes { get; set; }
	}
}
