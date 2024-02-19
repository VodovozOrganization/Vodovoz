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

		public async Task<string> Login()
		{
			var response = await _httpClient.GetAsync($"API/Login?login={_edoSettings.TaxcomLogin}&password={_edoSettings.TaxcomPassword}");
			var responseBody = await response.Content.ReadAsStringAsync();

			return responseBody;
		}
	}
}
