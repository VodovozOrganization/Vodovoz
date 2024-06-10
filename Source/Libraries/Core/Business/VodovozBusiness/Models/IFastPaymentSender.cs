using System.Threading.Tasks;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Models
{
	public interface IFastPaymentSender
	{
		Task<FastPaymentResult> SendFastPaymentUrlAsync(int orderId, string phoneNumber, bool isQr);
	}
}
