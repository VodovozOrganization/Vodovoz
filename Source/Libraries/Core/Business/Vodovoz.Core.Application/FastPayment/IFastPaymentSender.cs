using System.Threading.Tasks;

namespace Vodovoz.Core.Application.FastPayment
{
	public interface IFastPaymentSender
	{
		Task<FastPaymentResult> SendFastPaymentUrlAsync(int orderId, string phoneNumber, bool isQr);
	}
}
