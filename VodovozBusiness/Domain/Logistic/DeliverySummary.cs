using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Domain.Logistic
{
	public class DeliverySummary
	{
		public DeliverySummary(){}
		public DeliverySummary(string status, int ordersCount, IList<DeliverySummaryNode> orders)
		{
			name = status;
			addressCount = ordersCount;
			bottles = orders.Sum(x=>x.Bottles);
		}

		private string name;

		public string Name
		{
			get => name;
			set => name = value;
		}

		private int addressCount;

		public int AddressCount
		{
			get => addressCount;
			set => addressCount = value;
		}

		private decimal bottles;

		public decimal Bottles
		{
			get => bottles;
			set => bottles = value;
		}
	}
}