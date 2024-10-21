using CryptoPro.Security.Cryptography.Pkcs;
using CryptoPro.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrueMark.Api.Options;
using TrueMark.Contracts.Auth;

namespace TrueMark.Api.Services.Authorization;

public class AuthorizationService : IAuthorizationService
{
	private static HttpClient _httpClient;
	private readonly IOptions<TrueMarkApiOptions> _options;
	private readonly ILogger<AuthorizationService> _logger;
	private readonly HashSet<AuthorizationTokenCache> _tokenCacheList = new();

	public AuthorizationService(IOptions<TrueMarkApiOptions> options, IHttpClientFactory httpClientFactory, ILogger<AuthorizationService> logger)
	{
		_httpClient = httpClientFactory.CreateClient();
		_httpClient.BaseAddress = new Uri(options.Value.ExternalTrueMarkBaseUrl);
		_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		_options = options ?? throw new ArgumentNullException(nameof(options)); ;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<string> Login(string сertificateThumbPrint, string inn)
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

		var authKeyResponse = await _httpClient.GetStreamAsync(authUrn);
		var authKey = await JsonSerializer.DeserializeAsync<AuthKeyResponseDto>(authKeyResponse);

		var currentCert = _options.Value.OrganizationCertificates.SingleOrDefault(c => c.CertificateThumbPrint == сertificateThumbPrint && c.Inn == inn);
		var sign = await CreateAttachedSignedCmsWithStore2012_256(authKey.Data, false, currentCert.CertPath, currentCert.CertPwd);

		var tokenRequest = new TokenRequestDto
		{
			Uuid = authKey.Uuid,
			Data = Convert.ToBase64String(sign),
			Inn = inn
		};

		var serializedTokenRequest = JsonSerializer.Serialize(tokenRequest);
		var tokenContent = new StringContent(serializedTokenRequest, Encoding.UTF8, "application/json");
		var tokenResponseBody = await _httpClient.PostAsync(signInUrn, tokenContent);

		if(tokenResponseBody.IsSuccessStatusCode)
		{
			var responseContent = await tokenResponseBody.Content.ReadAsStreamAsync();
			var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponseDto>(responseContent);

			if(tokenResponse != null)
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

	public Task<byte[]> CreateAttachedSignedCmsWithStore2012_256(string data, bool isDeatchedSign, string certPath, string certPwd)
	{
		byte[] signature;

		byte[] dataBytes = Encoding.UTF8.GetBytes(data);

		using(var gostCert = new CpX509Certificate2(certPath, certPwd, X509KeyStorageFlags.EphemeralKeySet))
		{
			var contentInfo = new ContentInfo(dataBytes);
			var signedCms = new CpSignedCms(contentInfo, isDeatchedSign);
			CpCmsSigner cmsSigner = new CpCmsSigner(gostCert);
			signedCms.ComputeSignature(cmsSigner);
			signature = signedCms.Encode();
		}

		return Task.FromResult(signature);
	}
}
