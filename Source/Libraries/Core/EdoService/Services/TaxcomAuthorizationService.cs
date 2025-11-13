using System;
using System.Net.Http;
using System.Threading.Tasks;
using Vodovoz.Settings.Edo;

namespace EdoService.Library.Services
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
				BaseAddress = new Uri(edoSettings.TaxcomBaseAddressUri)
			};

			_httpClient.DefaultRequestHeaders.Add("Integrator-Id", _edoSettings.TaxcomIntegratorId);
		}

		public async Task<string> Login(string login, string password)
		{
			var response = await _httpClient.GetAsync($"API/Login?login={login}&password={password}");
			var responseBody = await response.Content.ReadAsStringAsync();

			return responseBody;
		}
	}
}
