using System.Threading.Tasks;

namespace TrueMarkApi.Services.Authorization
{
	public interface IAuthorizationService
	{
		Task<string> Login(string сertificateThumbPrint, string inn);
		Task<byte[]> CreateAttachedSignedCmsWithStore2012_256(string data, bool isDeatchedSign, string certPath, string certPwd);
	}
}
