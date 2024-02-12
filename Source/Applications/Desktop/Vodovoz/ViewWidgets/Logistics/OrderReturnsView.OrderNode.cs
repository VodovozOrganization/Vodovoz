using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	public partial class OrderReturnsView
	{
		private partial class OrderNode : PropertyChangedBase
		{
			private Counterparty _client;
			private DeliveryPoint _deliveryPoint;

			public Counterparty Client
			{
				get => _client;
				set => SetField(ref _client, value);
			}

			public DeliveryPoint DeliveryPoint
			{
				get => _deliveryPoint;
				set => SetField(ref _deliveryPoint, value);
			}

			private Order BaseOrder { get; set; }

			public OrderNode(Order order)
			{
				DeliveryPoint = order.DeliveryPoint;
				Client = order.Client;
				BaseOrder = order;
			}

			public ChangedType CompletedChange
			{
				get
				{
					if(Client == null || DeliveryPoint == null)
					{
						return ChangedType.None;
					}

					if(Client.Id == BaseOrder.Client.Id && DeliveryPoint.Id != BaseOrder.DeliveryPoint.Id)
					{
						return ChangedType.DeliveryPoint;
					}

					if(Client.Id != BaseOrder.Client.Id)
					{
						return ChangedType.Both;
					}

					return ChangedType.None;
				}
			}
		}
	}
}
