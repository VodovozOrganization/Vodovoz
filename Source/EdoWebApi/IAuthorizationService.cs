using System.Threading.Tasks;

namespace EdoWebApi
{
	public interface IAuthorizationService
	{
		Task<string> Login();
	}
}