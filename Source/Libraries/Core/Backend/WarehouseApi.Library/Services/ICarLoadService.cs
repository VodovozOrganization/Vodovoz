using System.Threading.Tasks;
using Vodovoz.Errors;
using WarehouseApi.Contracts.Responses;

namespace WarehouseApi.Library.Services
{
	public interface ICarLoadService
	{
		Task<Result<StartLoadResponse>> StartLoad(int documentId, string userLogin, string accessToken);
		Task<Result<GetOrderResponse>> GetOrder(int orderId);
		Task<Result<AddOrderCodeResponse>> AddOrderCode(int orderId, int nomenclatureId, string code, string userLogin);
		Task<Result<ChangeOrderCodeResponse>> ChangeOrderCode(int orderId, int nomenclatureId, string oldScannedCode, string newScannedCode, string userLogin);
		Task<Result<EndLoadResponse>> EndLoad(int documentId, string userLogin, string accessToken);
	}
}
