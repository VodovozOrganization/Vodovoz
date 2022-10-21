using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace EdoWebApi
{

	public class AuthorizationService : IAuthorizationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public AuthorizationService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task<string> Login()
		{
			var authorizationServiceSection = _configuration.GetSection("AuthorizationService");
			var login = authorizationServiceSection.GetValue<string>("Login");
			var password = authorizationServiceSection.GetValue<string>("Password");
			var response = await _httpClient.GetAsync($"API/Login?login={login}&password={password}");

			return await response.Content.ReadAsStringAsync();
		}
	}
}
