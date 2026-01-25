using System;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace Vodovoz.Application.Orders
{
	public class CheckOnlineOrderSum : ICheckOnlineOrderSum
	{
		protected CheckOnlineOrderSum(int nomenclatureId, decimal count, decimal price, decimal discount)
		{
			NomenclatureId = nomenclatureId;
			Count = count;
			Price = price;
			DiscountMoney = discount;
		}
		
		public int NomenclatureId { get; set; }
		public decimal Price { get; set; }
		public decimal Count { get; set; }
		public decimal DiscountMoney { get; set; }

		public decimal Sum => Math.Round(Count * Price - DiscountMoney, 2);
		public decimal CalculateDiscountMoney(decimal percentDiscount) =>
			Price * Count * percentDiscount / 100;
		
		public static CheckOnlineOrderSum Create(int nomenclatureId, decimal count, decimal price, decimal discount) =>
			new CheckOnlineOrderSum(nomenclatureId, count, price, discount);
	}
}
