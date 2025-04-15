using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Товар на продажу
	/// </summary>
	public class OrderSaleItemDto
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
		/// Количество
		/// </summary>
		public decimal Quantity { get; set; }

		/// <summary>
		/// Цена товара
		/// </summary>
		public decimal OrderItemPrice { get; set; }

		/// <summary>
		/// Полная цена товара
		/// </summary>
		public decimal TotalOrderItemPrice { get; set; }

		/// <summary>
		/// Объем тары
		/// </summary>
		public decimal? TareVolume { get; set; }

		/// <summary>
		/// Цвет пробки 19Л бутылки
		/// </summary>
		public string CapColor { get; set; }

		/// <summary>
		/// Нужно просканировать код
		/// </summary>
		public bool NeedScanCode { get; set; }

		/// <summary>
		/// Акция бутыль
		/// </summary>
		public bool IsBottleStock { get; set; }

		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		public bool IsDiscountInMoney { get; set; }

		/// <summary>
		/// Причина скидки
		/// </summary>
		public string DiscountReason { get; set; }

		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }

		/// <summary>
		/// Требуется дополнительный контроль водителя
		/// </summary>
		public bool IsNeedAdditionalControl { get; set; }

		/// <summary>
		/// GTIN товара
		/// </summary>
		public IEnumerable<string> Gtin { get; set; }

		/// <summary>
		/// Номера группы товарной продукции GTIN
		/// </summary>
		public IEnumerable<GroupGtinDto> GroupGtins { get; set; }

		/// <summary>
		/// Коды маркировки ЧЗ
		/// </summary>
		public IEnumerable<TrueMarkCodeDto> Codes { get; set; }
	}
}
