using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using CryptoPro.Security.Cryptography;
using CryptoPro.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using TaxcomEdo.Contracts.Authorization;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoAuthorizationService : IEdoAuthorizationService
	{
		private readonly HttpClient _httpClient;
		private readonly TaxcomEdoApiOptions _options;
		
		//TODO реализовать хранение срока действия токена
		private static readonly ConcurrentDictionary<string, EdoAuthorizationTokenState> _cache =
			new ConcurrentDictionary<string, EdoAuthorizationTokenState>();

		public EdoAuthorizationService(
			HttpClient httpClient,
			IOptions<TaxcomEdoApiOptions> options)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
		}
		
		public async Task<string> LoginAsync(
			string login,
			string password,
			HttpRequestType requestType = HttpRequestType.Post,
			CancellationToken cancellationToken = default)
		{
			switch(requestType)
			{
				case HttpRequestType.Get:
					return await LoginAsync(login, password, cancellationToken);
				case HttpRequestType.Post:
					
					var loginDto = new LoginDto
					{
						Login = login,
						Password = password
					};
					
					return await LoginAsync(loginDto, cancellationToken);
				default:
					throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null);
			}
		}
		
		public async Task<string> CertificateLoginAsync(byte[] certificateData, CancellationToken cancellationToken = default)
		{
			if(_cache.TryGetValue(_options.EdxClientId, out var cachedToken))
			{
				return cachedToken.Token;
			}
			
			var content = PrepareByteArrayContent(certificateData);
			
			var response = await _httpClient.PostAsync(ExternalApiConstants.CertificateLoginUri, content, cancellationToken);

			if(response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
				var key = DecryptMessage(responseBody);
				_cache.TryAdd(_options.EdxClientId, EdoAuthorizationTokenState.Create(key, DateTime.Now));

				return key;
			}

			return $"{response.ReasonPhrase} {response.StatusCode}";
		}
		
		private async Task<string> LoginAsync(string login, string password, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(login, nameof(login))
				.AddParameter(password, nameof(password))
				.ToString();
			
			var response = await _httpClient.GetAsync(ExternalApiConstants.LoginUri + query, cancellationToken);
			var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
			
			return DecryptMessage(responseBody);
		}
		
		private async Task<string> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.PostAsJsonAsync(ExternalApiConstants.LoginUri, loginDto, cancellationToken);
			var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
			
			return DecryptMessage(responseBody);
		}
		
		private ByteArrayContent PrepareByteArrayContent(byte[] data)
		{
			var content = new ByteArrayContent(data);
			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pkcs7-mime");
			content.Headers.ContentLength = data.Length;
			return content;
		}
		
		private string DecryptMessage(byte[] message)
		{
			byte[] signature;
			var thumbprint = _options.CertificateThumbprint;

			var certificate = GetGost2012_256Certificate(thumbprint, StoreName.My, StoreLocation.CurrentUser)
				?? GetGost2012_256Certificate(thumbprint, StoreName.My, StoreLocation.LocalMachine);
			
			if(certificate is null)
			{
				throw new InvalidOperationException($"Не найден сертификат для подписи thumbprint: {thumbprint} ");
			}
			
			using(certificate)
			{
				return Encoding.ASCII.GetString(DecryptMsg(message, certificate));
			}
		}
		
		private static byte[] DecryptMsg(byte[] encodedEnvelopedCms, CpX509Certificate2 cert)
		{
			// Создаем объект для декодирования и расшифрования.
			var envelopedCms = new CpEnvelopedCms();

			// Декодируем сообщение.
			envelopedCms.Decode(encodedEnvelopedCms);

			// Расшифровываем сообщение для единственного 
			// получателя.
			envelopedCms.Decrypt(new CpX509Certificate2Collection(cert));

			// После вызова метода Decrypt в свойстве ContentInfo 
			// содержится расшифрованное сообщение.
			return envelopedCms.ContentInfo.Content;
		}
		
		private static CpX509Certificate2 GetGost2012_256Certificate(
			string thumbprint,
			StoreName storeName,
			StoreLocation storeLocation)
		{
			if(string.IsNullOrWhiteSpace(thumbprint))
			{
				throw new ArgumentNullException(nameof(thumbprint));
			}
			
			using var store = new CpX509Store(storeName, storeLocation);
			store.Open(OpenFlags.ReadOnly);
			
			var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
				
			return certificates.Count switch
			{
				1 => certificates[0],
				> 1 => throw new InvalidOperationException($"Найдено больше одного сертификата с отпечатком {thumbprint}"),
				_ => null
			};
		}
	}

	/// <summary>
	/// Тип запроса
	/// </summary>
	public enum HttpRequestType
	{
		/// <summary>
		/// GET запрос
		/// </summary>
		Get,
		/// <summary>
		/// POST запрос
		/// </summary>
		Post,
	}
}
