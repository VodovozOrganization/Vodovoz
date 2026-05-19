using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class OnlineOrdersTemplatesData
	{
		public IList<OnlineOrderTemplateData> Templates { get; private set; }
		public ILookup<int, OnlineOrderTemplateWeekday> WeekdaysLookup { get; private set; }
		public ILookup<int, OnlineOrderTemplateProduct> ProductsLookup{ get; private set; }

		public static OnlineOrdersTemplatesData Create(
			IList<OnlineOrderTemplateData> templates,
			ILookup<int, OnlineOrderTemplateWeekday> weekdaysLookup,
			ILookup<int, OnlineOrderTemplateProduct> productsLookup
			)
		{
			return new OnlineOrdersTemplatesData
			{
				Templates = templates,
				WeekdaysLookup = weekdaysLookup,
				ProductsLookup = productsLookup,
			};
		}
	}
}
