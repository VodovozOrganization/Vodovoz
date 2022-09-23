using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;

namespace RoboAtsService.OrderValidation
{
	public class OrderMultiValidator
	{
		private List<OrderValidatorBase> _orderValidators = new List<OrderValidatorBase>();

		public virtual void AddValidator(OrderValidatorBase orderValidator)
		{
			if(_orderValidators.Contains(orderValidator))
			{
				return;
			}
			_orderValidators.Add(orderValidator);
		}

		public OrderValidationResult ValidateOrders(IEnumerable<Order> orders)
		{
			RunValidators(orders);

			var resultOrders = new List<Order>();

			foreach(var order in orders)
			{
				if(_orderValidators.All(x => x.IsValid(order)))
				{
					resultOrders.Add(order);
				};
			}

			if(resultOrders.Any())
			{
				return new OrderValidationResult(resultOrders);
			}
			else
			{
				var problemMessages = _orderValidators.SelectMany(x => x.GetProblemMessages(orders));
				return new OrderValidationResult(problemMessages);
			}
		}

		private void RunValidators(IEnumerable<Order> orders)
		{
			var validatorsCount = _orderValidators.Count;
			var tasks = new Task[validatorsCount];

			for(int i = 0; i < validatorsCount; i++)
			{
				var validator = _orderValidators[i];
				tasks[i] = Task.Run(() => validator.Validate(orders));
			}
			Task.WaitAll(tasks);
		}
	}
}
