using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrueMarkApi.Dto.Auth;
using TrueMarkApi.Models;

namespace TrueMarkApi.Services.Authorization
{
	public class AuthorizationService:IAuthorizationService
	{
		private static HttpClient _httpClient;
		private readonly ILogger<AuthorizationService> _logger;
		private readonly HashSet<AuthorizationTokenCache> _tokenCacheList = new();

		public AuthorizationService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AuthorizationService> logger)
		{
			var apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");

			_httpClient = httpClientFactory.CreateClient();
			_httpClient.BaseAddress = new Uri(apiSection.GetValue<string>("ExternalTrueApiBaseUrl"));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<string> Login(string сertificateThumbPrint)
		{
			var authUrn = "auth/key";
			var signInUrn = "auth/simpleSignIn";

			var cachedToken = _tokenCacheList.FirstOrDefault(tc => tc.CertificateThumbPrint == сertificateThumbPrint);

			if(cachedToken is { IsTokenFresh: true } && !string.IsNullOrWhiteSpace(cachedToken.Token))
			{
				_logger.LogInformation($"Возвращаем кешированный токен, всего кешировано: {_tokenCacheList.Count}");
				return cachedToken.Token;
			}

			_tokenCacheList.Remove(cachedToken);

			_logger.LogInformation("Токен авторизации устарел, получаем новый...");

			var authKeyResponse = await _httpClient.GetAsync(authUrn);
			var authKeyStream = await authKeyResponse.Content.ReadAsStreamAsync();
			var authKey = await JsonSerializer.DeserializeAsync<AuthKeyResponseDto>(authKeyStream);
			var authKeyDataInBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(authKey.Data));

			var signModel = new SignModel(сertificateThumbPrint, authKeyDataInBase64String, true);

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

				if (tokenResponse != null)
				{
					var tokenCache = new AuthorizationTokenCache
					{
						CertificateThumbPrint = сertificateThumbPrint,
						TokenCreateTime = DateTime.Now,
						Token = tokenResponse.Token
					};

					_tokenCacheList.Add(tokenCache);

					return tokenCache.Token;
				}
			}

			_logger.LogError($"Ошибка при получении токена авторизации в ЧЗ: Http code {tokenResponseBody.StatusCode}, причина {tokenResponseBody.ReasonPhrase}");

			return null;
		}
	}
}
