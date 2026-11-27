using Vodovoz.Core.Data.Sale;

namespace CustomerOrdersApi.Library.V8.Dto.Sale
{
	/// <summary>
	/// Позиция заказа после списания бонусов
	/// </summary>
	public class BonusSaleItemDto : OnlineOrderItemWithDiscountDetailsDto
	{
		/// <summary>
		/// Бонусы, списанные на позицию
		/// </summary>
		public int BonusesWrittenOff { get; set; }
	}
}
