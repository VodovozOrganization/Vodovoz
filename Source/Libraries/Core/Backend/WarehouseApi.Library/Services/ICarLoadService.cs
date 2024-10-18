using System.Threading.Tasks;
using WarehouseApi.Contracts.Responses;
using WarehouseApi.Library.Common;

namespace WarehouseApi.Library.Services
{
	public interface ICarLoadService
	{
		Task<RequestProcessingResult<StartLoadResponse>> StartLoad(int documentId, string userLogin, string accessToken);
		Task<RequestProcessingResult<GetOrderResponse>> GetOrder(int orderId);
		Task<RequestProcessingResult<AddOrderCodeResponse>> AddOrderCode(int orderId, int nomenclatureId, string code, string userLogin);
		Task<RequestProcessingResult<ChangeOrderCodeResponse>> ChangeOrderCode(int orderId, int nomenclatureId, string oldScannedCode, string newScannedCode, string userLogin);
		Task<RequestProcessingResult<EndLoadResponse>> EndLoad(int documentId, string userLogin, string accessToken);
	}
}
