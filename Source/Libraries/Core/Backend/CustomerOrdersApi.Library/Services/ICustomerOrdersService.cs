using CustomerOrdersApi.Library.Dto.Orders;

namespace CustomerOrdersApi.Library.Services
{
	public interface ICustomerOrdersService
	{
		bool ValidateSignature(OnlineOrderInfoDto onlineOrderInfoDto, out string generatedSignature);
	}
}
