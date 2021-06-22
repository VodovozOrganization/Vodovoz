using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class UndeliveryOrderNode
	{
		private int _orderId;
		public int OrderId
		{
			get => _orderId;
			set => _orderId = value;
		}

		private GuiltyTypes _guiltyTypes;
		public GuiltyTypes GuiltySide
		{
			get => _guiltyTypes;
			set => _guiltyTypes = value;
		}

		private int _bottles;

		public int Bottles
		{
			get => _bottles;
			set => _bottles = value;
		}

		private DeliveryPoint _deliveryPoint;
		
		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => _deliveryPoint = value;
		}
	}
}