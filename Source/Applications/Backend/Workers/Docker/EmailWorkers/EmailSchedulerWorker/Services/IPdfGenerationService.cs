using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace EmailSchedulerWorker.Services
{
	public interface IPdfGenerationService
	{
		byte[] GenerateDebtNotificationPdfAsync(Counterparty client, IEnumerable<Order> order);
	}
}
