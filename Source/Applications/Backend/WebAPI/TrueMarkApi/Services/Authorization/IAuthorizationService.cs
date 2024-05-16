using System.Threading.Tasks;

namespace TrueMarkApi.Services.Authorization
{
	public interface IAuthorizationService
	{
		Task<string> Login(string сertificateThumbPrint, string inn);
	}
}
