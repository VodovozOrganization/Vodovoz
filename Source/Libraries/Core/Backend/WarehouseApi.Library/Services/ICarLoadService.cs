using System.Threading.Tasks;
using Vodovoz.Errors;
using WarehouseApi.Contracts.Responses;

namespace WarehouseApi.Library.Services
{
	public interface ICarLoadService
	{
		Task<Result<StartLoadResponse>> StartLoad(int documentId);
	}
}
