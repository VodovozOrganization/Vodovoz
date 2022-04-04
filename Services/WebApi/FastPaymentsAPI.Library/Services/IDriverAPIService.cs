using System.Threading.Tasks;

namespace FastPaymentsAPI.Library.Services;

public interface IDriverAPIService
{
	Task NotifyOfFastPaymentStatusChangedAsync(int orderId);
}
