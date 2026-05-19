using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	public class AggregateOnlineOrderTemplateInfo
	{
		public OnlineOrderTemplateInfo Template { get; private set; }
		public Counterparty Counterparty { get; private set; }
		public DeliveryPoint DeliveryPoint { get; private set; }
		public IEnumerable<string> Weekdays { get; private set; }
		public IEnumerable<OnlineOrderTemplateProduct> Products { get; private set; }

		public static AggregateOnlineOrderTemplateInfo Create(
			OnlineOrderTemplateInfo template,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			IEnumerable<string> weekdays,
			IEnumerable<OnlineOrderTemplateProduct> products
		)
		{
			return new AggregateOnlineOrderTemplateInfo
			{
				Template = template,
				Counterparty = counterparty,
				DeliveryPoint = deliveryPoint,
				Weekdays = weekdays,
				Products = products
			};
		}
	}
}
