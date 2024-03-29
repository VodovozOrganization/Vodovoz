﻿using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class OrdersCountNode: PropertyChangedBase
	{
		private OrderStatus orderStatus;

		public OrderStatus OrderStatus
		{
			get => orderStatus;
			set => SetField(ref orderStatus, value);
		}

		private int id;
		public int Id 
		{
			get => id;
			set => SetField(ref id, value);
		}
	}
}