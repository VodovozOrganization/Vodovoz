using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace Receipt.Dispatcher.Tests.Fixtures
{
	public class OrderEntityFixture : OrderEntity
	{
		public void SetClient(CounterpartyEntity client) => Client = client;
		public void SetDeliveryDate(DateTime? deliveryDate) => DeliveryDate = deliveryDate;
	}
}
