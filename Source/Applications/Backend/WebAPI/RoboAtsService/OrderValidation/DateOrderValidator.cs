﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;

namespace RoboatsService.OrderValidation
{
	public sealed class DateOrderValidator : OrderValidatorBase
	{
		private readonly RoboatsSettings _roboatsSettings;
		private int _ordersInMonths => _roboatsSettings.OrdersInMonths;

		public DateOrderValidator(RoboatsSettings roboatsSettings)
		{
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
		}

		public override IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders)
		{
			var result = orders.Where(x => !IsValid(x)).Select(x => $"Заказ №{x.Id} был оформлен более {_ordersInMonths} месяцев назад");
			return result;
		}

		public override void Validate(IEnumerable<Order> orders)
		{
			foreach(var order in orders)
			{
				if(order.DeliveryDate >= DateTime.Today.AddMonths(-_ordersInMonths))
				{
					AddValidOrder(order);
				}
			}
		}
	}
}
