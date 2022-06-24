using FastPaymentsAPI.Library.DTO_s.Requests;
using System.Threading.Tasks;

namespace FastPaymentsAPI.Library.Services
{
	public interface IVodovozSiteNotificationService
	{
		Task NotifyOfFastPaymentStatusChangedAsync(VodovozSiteNotificationPaymentRequestDto paymentNotificationDto);
	}
}
