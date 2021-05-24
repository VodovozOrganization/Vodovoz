using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class DeliverySummary: PropertyChangedBase
	{
		public DeliverySummary(){}
		public DeliverySummary(OrderStatus status, IList<Order> orders)
		{
			name = status.GetEnumTitle();
			addressCount = orders.Count;
			bottles = orders.Sum(x=>x.Total600mlBottlesToDeliver + x.Total6LBottlesToDeliver + x.Total19LBottlesToDeliver);;
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