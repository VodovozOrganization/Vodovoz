using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public abstract class OrderValidatorBase
	{
		private Dictionary<int, Order> _validOrders = new Dictionary<int, Order>();

		public abstract IEnumerable<string> GetProblemMessages(IEnumerable<Order> orders);

		public abstract void Validate(IEnumerable<Order> orders);

		public virtual bool IsValid(Order order)
		{
			return _validOrders.ContainsKey(order.Id);
		}

		protected virtual void AddValidOrder(Order order)
		{
			if(!_validOrders.ContainsKey(order.Id))
			{
				_validOrders.Add(order.Id, order);
			}
		}

		protected virtual void RemoveOrderFromValid(Order order)
		{
			if(_validOrders.ContainsKey(order.Id))
			{
				_validOrders.Remove(order.Id);
			}
		}
	}
}
