using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class UndeliveryOrderNode
	{
		private int _oldOrderId;
		public int OldOrderId
		{
			get => _oldOrderId;
			set => _oldOrderId = value;
		}
		
		private int _newOrderId;
		public int NewOrderId
		{
			get => _newOrderId;
			set => _newOrderId = value;
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