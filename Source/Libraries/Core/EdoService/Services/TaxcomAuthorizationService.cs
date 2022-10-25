using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Vodovoz.Services;

namespace EdoService.Services
{
	public class TaxcomAuthorizationService : IAuthorizationService
	{
		private static HttpClient _httpClient;
		private readonly IEdoSettings _edoSettings;

		public TaxcomAuthorizationService(IEdoSettings edoSettings)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));

			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri(edoSettings.TaxcomBaseAddressUri),
				Timeout = TimeSpan.FromSeconds(5)
			};

			_httpClient.DefaultRequestHeaders.Add("Integrator-Id", _edoSettings.TaxcomIntegratorId);
		}

		public async Task<string> Login()
		{
			//var authorizationServiceSection = _configuration.GetSection("TaxcomAuthorizationService");
			var login = _edoSettings.TaxcomLogin;
			var password = _edoSettings.TaxcomPassword;
			var response = await _httpClient.GetAsync($"API/Login?login={login}&password={password}");
			var result = await response.Content.ReadAsStringAsync();
			return result;
		}
	}
}
