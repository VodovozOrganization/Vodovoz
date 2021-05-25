using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public interface ISmsPaymentServiceAPIHelper
	{
		Task SendPayment(int orderId, string phoneNumber);
	}
}