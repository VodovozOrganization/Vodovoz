using System.Threading.Tasks;

namespace TrueApi.Services
{
	public interface IAuthorizationService
	{
		Task<string> Login();
	}
}
