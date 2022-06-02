using Vodovoz.Domain.Payments;

namespace Vodovoz.Factories
{
	public interface IPaymentFromAvangardFactory
	{
		PaymentFromAvangard CreateNewPaymentFromAvangard(ImportPaymentsFromAvangardSbpNode node);
		ImportPaymentsFromAvangardSbpNode CreateImportPaymentsFromAvangardSbpNode(string[] data);
	}
}
