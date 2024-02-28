using DriverApi.Contracts.V4;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public interface IFastPaymentsServiceAPIHelper
	{
		Task<QRResponseDTO> SendPaymentAsync(int orderId);
	}
}
