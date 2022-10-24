using System.Threading.Tasks;

namespace EdoApi.Library.Services
{
	public interface IAuthorizationService
	{
		Task<string> Login();
	}
}
