using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class AmountOnDateNode
	{
		public int Amount { get; set; }
		public DateTime DateTime { get; set; }

		public static int GetAmountOnDate(IEnumerable<AmountOnDateNode> list, DateTime date)
		{
			return list.FirstOrDefault(x => x.DateTime == date)?.Amount ?? 0;
		}

		public static int GetAmountOnDate(IDictionary<DateTime, int> dict, DateTime date)
		{
			dict.TryGetValue(date, out var amount);
			return amount;
		}
	}
}
