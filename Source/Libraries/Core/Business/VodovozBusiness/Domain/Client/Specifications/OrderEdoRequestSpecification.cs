using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class OrderEdoRequestSpecification
	{
		public static ExpressionSpecification<OrderEdoRequest> CreateForOrderId(int orderId)
			=> new ExpressionSpecification<OrderEdoRequest>(
				x => x.Order.Id == orderId);
	}
}
