using System.Threading.Tasks;
using Vodovoz.Errors;
using WarehouseApi.Contracts.Responses;

namespace WarehouseApi.Library.Services
{
	public interface ICarLoadService
	{
		Task<Result<StartLoadResponse>> StartLoad(int documentId);
		Task<Result<GetOrderResponse>> GetOrder(int orderId);
		Task<Result<AddOrderCodeResponse>> AddOrderCode(int orderId, int nomenclatureId, string code);
		Task<Result<AddOrderCodeResponse>> ChangeOrderCode(int orderId, int nomenclatureId, string oldScannedCode, string newScannedCode);
		Task<Result<EndLoadResponse>> EndLoad(int documentId);
	}
}
