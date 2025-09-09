using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure;
using Microsoft.Extensions.Options;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoAuthorizationService : IEdoAuthorizationService
	{
		private readonly HttpClient _httpClient;
		private readonly TaxcomEdoApiOptions _options;

		public EdoAuthorizationService(
			HttpClient httpClient,
			IOptions<TaxcomEdoApiOptions> options)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
		}
		
		public async Task<string> Login(string login, string password)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(login, nameof(login))
				.AddParameter(password, nameof(password))
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.AuthorizationUri.LoginUri + query);
			var responseBody = await response.Content.ReadAsStringAsync();

			return responseBody;
		}
		
		public async Task<string> CertificateLogin(byte[] certificateData)
		{
			var content = new ByteArrayContent(certificateData);
			
			var response = await _httpClient.PostAsync(_options.AuthorizationUri.CertificateLoginUri, content);
			var tokenData = DecryptMessage();
			var token = Encoding.ASCII.GetString(tokenData);
			
			return token;
		}
		
		private byte[] DecryptMessage(byte[] message)
		{
			try
			{
				var encryption = new Encryption();
				return encryption.DecryptDocument(message);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("There is an error while decrypting of the message: {0}", ex.Message));
			}
		}
	}

	public interface IEdoAuthorizationService
	{
		Task<string> Login(string login, string password);
		Task<string> CertificateLogin(byte[] certificateData);
	}
}
