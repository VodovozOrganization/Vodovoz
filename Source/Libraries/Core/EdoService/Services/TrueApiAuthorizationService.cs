using EdoService.Dto;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vodovoz.Services;

namespace EdoService.Services
{
	public class TrueApiAuthorizationService : IAuthorizationService
	{
		private static HttpClient _httpClient;
		private readonly IEdoSettings _edoSettings;

		public TrueApiAuthorizationService(IEdoSettings edoSettings)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));

			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri(edoSettings.TrueApiBaseAddressUri),
				Timeout = TimeSpan.FromSeconds(5)
			};
		}


		// Не сделано, жду ЭЦП
		public async Task<string> Login()
		{
			var response = await _httpClient.GetAsync($"auth/key");
			var responseBody = await response.Content.ReadAsStreamAsync();

			var registrationResult = JsonSerializer.Deserialize<TrueApiAuthDto>(responseBody);

			string content = JsonSerializer.Serialize(registrationResult);

			using(HttpClient httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Accept.Clear();
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
				HttpResponseMessage httpResponse = await httpClient.PostAsync($"{_httpClient.BaseAddress}auth/simpleSignIn", httpContent);

				var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
			}

			return null;
		}
	}
}
