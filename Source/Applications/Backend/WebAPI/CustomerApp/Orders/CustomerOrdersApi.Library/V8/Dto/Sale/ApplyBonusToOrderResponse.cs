using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V8.Dto.Sale
{
	/// <summary>
	/// Результат расчёта баланса, доступного списания и начисления бонусов
	/// </summary>
	public class ApplyBonusToOrderResponse
	{
		/// <summary>
		/// Данные по бонусам клиента в заказе
		/// </summary>
		public BonusInfoDto BonusInfo { get; set; }

		/// <summary>
		/// Итог до списания бонусов
		/// </summary>
		public decimal OrderSumBeforeBonuses { get; set; }

		/// <summary>
		/// Итог к оплате
		/// </summary>
		public decimal OrderSum { get; set; }

		/// <summary>
		/// Позиции заказа после списания бонусов
		/// </summary>
		public IEnumerable<BonusSaleItemDto> SaleItems { get; set; }
	}
}
