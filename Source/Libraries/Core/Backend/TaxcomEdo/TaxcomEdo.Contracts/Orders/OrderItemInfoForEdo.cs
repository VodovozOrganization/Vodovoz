using System;
using TaxcomEdo.Contracts.Goods;

namespace TaxcomEdo.Contracts.Orders
{
	/// <summary>
	/// Информация о строке заказа для ЭДО(электронного документооборота)
	/// </summary>
	public class OrderItemInfoForEdo
	{
		/// <summary>
		/// Id строки заказа
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Информация о номенклатуре <see cref="NomenclatureInfoForEdo"/>
		/// </summary>
		public NomenclatureInfoForEdo NomenclatureInfoForEdo { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Фактическое количество
		/// </summary>
		public decimal? ActualCount { get; set; }
		/// <summary>
		/// Включая НДС
		/// </summary>
		public decimal? IncludeNDS { get; set; }
		/// <summary>
		/// Налоговая ставка
		/// </summary>
		public decimal? ValueAddedTax { get; set; }
		/// <summary>
		/// Скидка в рублях
		/// </summary>
		public virtual decimal DiscountMoney { get; set; }
		
		public virtual decimal CurrentNDS => IncludeNDS ?? 0;
		public decimal CurrentCount => ActualCount ?? Count;
		public decimal ActualSum => Math.Round(Price * CurrentCount - DiscountMoney, 2);
		public decimal SumWithoutVat => Math.Round(Price * CurrentCount - CurrentNDS - DiscountMoney, 2);
		public decimal PriceWithoutVat =>
			CurrentCount == default
				? 0
				: Math.Round((Price * CurrentCount - CurrentNDS - DiscountMoney) / CurrentCount, 2);
	}
}
