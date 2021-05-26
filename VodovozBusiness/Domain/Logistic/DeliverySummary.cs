using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class DeliverySummary: PropertyChangedBase
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
			set => SetField(ref name, value);
		}

		private int addressCount;

		public int AddressCount
		{
			get => addressCount;
			set => SetField(ref addressCount, value);
		}

		private decimal bottles;

		public decimal Bottles
		{
			get => bottles;
			set => SetField(ref bottles, value);
		}
	}
}