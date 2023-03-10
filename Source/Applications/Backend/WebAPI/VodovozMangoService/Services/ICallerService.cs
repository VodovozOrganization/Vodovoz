using System.Threading.Tasks;

namespace VodovozMangoService.Services
{
	public interface ICallerService
	{
		Task<Caller> GetExternalCaller(string number);
	}
}