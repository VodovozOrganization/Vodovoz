using Vodovoz.Domain.Payments;
using Vodovoz.Nodes;

namespace Vodovoz.Factories
{
	public interface IPaymentFromAvangardFactory
	{
		PaymentFromAvangard CreateNewPaymentFromAvangard(AvangardOperation node);
	}
}
