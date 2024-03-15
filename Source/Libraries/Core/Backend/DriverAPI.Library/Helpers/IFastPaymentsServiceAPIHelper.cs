using FastPaymentsApi.Contracts.Responses;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public interface IFastPaymentsServiceAPIHelper
	{
		Task<QRResponseDTO> SendPaymentAsync(int orderId);
	}
}
