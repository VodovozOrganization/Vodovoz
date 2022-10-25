using System.Threading.Tasks;

namespace EdoService.Services
{
	public interface IAuthorizationService
	{
		Task<string> Login();
	}
}
