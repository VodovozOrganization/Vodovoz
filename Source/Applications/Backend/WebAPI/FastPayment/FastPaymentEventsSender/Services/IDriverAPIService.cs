using System.Threading.Tasks;

namespace FastPaymentEventsSender.Services
{
	public interface IDriverAPIService
	{
		Task NotifyOfFastPaymentStatusChangedAsync(int orderId);
	}
}
