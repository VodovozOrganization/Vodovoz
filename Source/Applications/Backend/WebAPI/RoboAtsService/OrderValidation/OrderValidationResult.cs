using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RoboatsService.OrderValidation
{
	public class OrderValidationResult
	{
		public OrderValidationResult(IEnumerable<Order> validOrders)
		{
			ValidOrders = validOrders;
			ProblemMessages = Enumerable.Empty<string>();
		}

		public OrderValidationResult(IEnumerable<string> problemMessages)
		{
			ValidOrders = Enumerable.Empty<Order>();
			ProblemMessages = problemMessages;
		}

		public bool HasValidOrders => ValidOrders.Any();
		public IEnumerable<Order> ValidOrders { get; }
		public IEnumerable<string> ProblemMessages { get; }
	}
}
