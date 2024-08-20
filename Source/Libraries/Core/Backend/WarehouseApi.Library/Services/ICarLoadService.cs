using Vodovoz.Errors;
using WarehouseApi.Contracts.Responses;

namespace WarehouseApi.Library.Services
{
	public interface ICarLoadService
	{
		Result<StartLoadResponse> StartLoad(int documentId);
	}
}