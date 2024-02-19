using System.Threading.Tasks;

namespace EdoService.Library.Services
{
	public interface IAuthorizationService
	{
		Task<string> Login();
	}
}
