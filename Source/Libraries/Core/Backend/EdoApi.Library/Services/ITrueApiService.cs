using System.Threading.Tasks;

namespace EdoApi.Library.Services
{
	public interface ITrueApiService
	{
		Task<bool> GetCounterpartyRegisteredInTrueApi(string inn, string productGroup);
	}
}
