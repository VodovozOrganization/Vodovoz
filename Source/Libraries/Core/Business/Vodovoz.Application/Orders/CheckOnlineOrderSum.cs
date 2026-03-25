using System;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace Vodovoz.Application.Orders
{
	/// <inheritdoc/>
	public class CheckOnlineOrderSum : ICheckOnlineOrderSum
	{
		protected CheckOnlineOrderSum(int nomenclatureId, decimal count, decimal price, decimal discount)
		{
			NomenclatureId = nomenclatureId;
			Count = count;
			Price = price;
			DiscountMoney = discount;
		}
		
		/// <inheritdoc/>
		public int NomenclatureId { get; set; }
		/// <inheritdoc/>
		public decimal Price { get; set; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
		/// <inheritdoc/>
		public decimal DiscountMoney { get; set; }
		/// <inheritdoc/>
		public decimal Sum => Math.Round(Count * Price - DiscountMoney, 2);
		/// <summary>
		/// Получение скидки в деньгах из процентной
		/// </summary>
		/// <param name="percentDiscount">Скидка в процентах</param>
		/// <returns>Скидка в деньгах</returns>
		public decimal CalculateDiscountMoney(decimal percentDiscount) =>
			Price * Count * percentDiscount / 100;
		
		public static CheckOnlineOrderSum Create(int nomenclatureId, decimal count, decimal price, decimal discount) =>
			new CheckOnlineOrderSum(nomenclatureId, count, price, discount);
	}
}
