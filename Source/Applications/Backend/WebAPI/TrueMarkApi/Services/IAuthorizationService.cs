using System.Threading.Tasks;

namespace TrueMarkApi.Services
{
	public interface IAuthorizationService
	{
		Task<string> Login(string _сertificateThumbPrint);
	}
}
