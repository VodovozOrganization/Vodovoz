using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;

namespace FastPaymentsAPI.Library.Services
{
	public interface IMobileAppNotificationService
	{
		Task NotifyOfFastPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto, string url);
	}
}
