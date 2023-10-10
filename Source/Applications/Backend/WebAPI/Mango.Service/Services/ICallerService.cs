using System.Threading.Tasks;

namespace Mango.Service.Services
{
	public interface ICallerService
	{
		Task<Caller> GetExternalCaller(string number);
	}
}