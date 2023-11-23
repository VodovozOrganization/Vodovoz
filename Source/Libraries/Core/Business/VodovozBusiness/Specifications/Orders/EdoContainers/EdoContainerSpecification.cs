using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Specifications.Orders.EdoContainers
{
	public class EdoContainerSpecification : ExpressionSpecification<EdoContainer>
	{
		private EdoContainerSpecification(Expression<Func<EdoContainer, bool>> expression)
			: base(expression)
		{
		}

		public static EdoContainerSpecification CreateForOrderId(int orderId)
			=> new EdoContainerSpecification(x => x.Order.Id == orderId);
	}
}
