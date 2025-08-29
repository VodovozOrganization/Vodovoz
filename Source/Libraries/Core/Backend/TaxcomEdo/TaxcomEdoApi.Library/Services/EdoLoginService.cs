using System;
using System.Threading.Tasks;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoLoginService
	{
		protected EdoLoginService(IEdoAuthorizationService edoAuthorizationService)
		{
			EdoAuthorizationService = edoAuthorizationService ?? throw new ArgumentNullException(nameof(edoAuthorizationService));
		}
		
		protected IEdoAuthorizationService EdoAuthorizationService { get; }
		
		protected async Task<string> CertificateLogin(byte[] certificateData)
		{
			var key = await EdoAuthorizationService.CertificateLogin(certificateData);
			return key;
		}
		
		protected async Task<string> Login(string login, string password)
		{
			var key = await EdoAuthorizationService.Login(login, password);
			return key;
		}
	}
}
