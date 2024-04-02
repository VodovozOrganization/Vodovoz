using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Отсканированные товары
	/// </summary>
	public class OrderScannedItemDto : ITrueMarkOrderItemScannedInfo
	{
		/// <summary>
		/// Номер товара в заказе
		/// </summary>
		public int OrderSaleItemId { get; set; }

		/// <summary>
		/// Коды бутылей
		/// </summary>
		public IEnumerable<string> BottleCodes { get; set; }

		/// <summary>
		/// Коды дефектных бутылей
		/// </summary>
		public IEnumerable<string> DefectiveBottleCodes { get; set; }
	}
}
