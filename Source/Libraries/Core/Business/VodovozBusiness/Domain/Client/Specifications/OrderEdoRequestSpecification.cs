using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class OrderEdoRequestSpecification
	{
		public static ExpressionSpecification<FormalEdoRequest> CreateForOrderId(int orderId)
			=> new ExpressionSpecification<FormalEdoRequest>(x => x.Order.Id == orderId);
	}
}
