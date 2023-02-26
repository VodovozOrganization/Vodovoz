using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrueMarkApi.Dto.Auth;
using TrueMarkApi.Models;

namespace TrueMarkApi.Services.Authorization
{
	public class AuthorizationService : IAuthorizationService
	{
		private static HttpClient _httpClient;
		private readonly List<AuthorizationTokenCache> _tokenCacheList = new();

		public AuthorizationService(IConfiguration configuration)
		{
			var apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");

			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri(apiSection.GetValue<string>("ExternalTrueApiBaseUrl"));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<string> Login(string сertificateThumbPrint)
		{
			var authUrn = "auth/key";
			var signInUrn = "auth/simpleSignIn";

			var cachedToken = _tokenCacheList.SingleOrDefault(tc => tc.CertificateThumbPrint == сertificateThumbPrint);

			if(cachedToken is { IsTokenFresh: true } && !string.IsNullOrWhiteSpace(cachedToken.Token))
			{
				return cachedToken.Token;
			}

			_tokenCacheList.Remove(cachedToken);

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

			return null;
		}
	}
}
