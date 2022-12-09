using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrueMarkApi.Dto.Auth;
using TrueMarkApi.Models;

namespace TrueMarkApi.Services
{
	public class AuthorizationService:IAuthorizationService
	{
		private static HttpClient _httpClient;
		private string _cachedToken;
		private DateTime _tokenTime;
		private readonly ILogger<AuthorizationService> _logger;

		public AuthorizationService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AuthorizationService> logger)
		{
			var apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");

			_httpClient = httpClientFactory.CreateClient();
			_httpClient.BaseAddress = new Uri(apiSection.GetValue<string>("ExternalTrueApiBaseUrl"));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<string> Login(string _сertificateThumbPrint)
		{
			var authUrn = "auth/key";
			var signInUrn = "auth/simpleSignIn";

			// Срок действия полученного токена не более 10 часов с момента получения.
			var isTokenFresh = (DateTime.Now - _tokenTime).TotalHours < 10;

			if(isTokenFresh && !string.IsNullOrWhiteSpace(_cachedToken))
			{
				return _cachedToken;
			}

			_logger.LogInformation($"Токен авторизации устарел, получаем новый...");

			var authKeyResponse = await _httpClient.GetAsync(authUrn);
			var authKeyStream = await authKeyResponse.Content.ReadAsStreamAsync();
			var authKey = await JsonSerializer.DeserializeAsync<AuthKeyResponseDto>(authKeyStream);
			var authKeyDataInBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(authKey.Data));

			var signModel = new SignModel(_сertificateThumbPrint, authKeyDataInBase64String, true);

			var tokenRequest = new TokenRequestDto
			{
				Uuid = authKey.Uuid,
				Data = signModel.Sign()
			};

			var serializedTokenRequest = JsonSerializer.Serialize(tokenRequest);
			var tokenContent = new StringContent(serializedTokenRequest, Encoding.UTF8, "application/json");
			var tokenResponseBody = await _httpClient.PostAsync(signInUrn, tokenContent);

			if(tokenResponseBody.IsSuccessStatusCode)
			{
				var responseContent = await tokenResponseBody.Content.ReadAsStreamAsync();
				var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponseDto>(responseContent);
				_tokenTime = DateTime.Now;

				return _cachedToken = tokenResponse?.Token;
			}

			_logger.LogError($"Ошибка при получении токена авторизации в ЧЗ: Http code {tokenResponseBody.StatusCode}, причина {tokenResponseBody.ReasonPhrase}");

			return null;
		}
	}
}
