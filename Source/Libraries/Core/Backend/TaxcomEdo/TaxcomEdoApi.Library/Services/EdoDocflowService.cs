using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoDocflowService : EdoLoginService, IEdoDocflowService
	{
		private readonly HttpClient _httpClient;
		private readonly IEdoAuthorizationService _edoAuthorizationService;
		private readonly TaxcomEdoApiOptions _options;

		public EdoDocflowService(
			HttpClient httpClient,
			IEdoAuthorizationService edoAuthorizationService,
			IOptions<TaxcomEdoApiOptions> options) : base(edoAuthorizationService)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_edoAuthorizationService = edoAuthorizationService ?? throw new ArgumentNullException(nameof(edoAuthorizationService));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
		}
		
		public async Task<bool> GetMessageListAsync(byte[] container, byte[] certificateData)
		{
			var key = await CertificateLogin(certificateData);
			var response = await _httpClient.PostAsync(_options.SendMessageUri, null);
			
			return response.IsSuccessStatusCode;
		}
		
		public async Task<bool> SendMessageAsync(byte[] container, byte[] certificateData)
		{
			var key = await CertificateLogin(certificateData);
			var response = await _httpClient.PostAsync(_options.SendMessageUri, null);
			
			return response.IsSuccessStatusCode;
		}
		
		public async Task<bool> GetDocflowUpdatesAsync(byte[] container, byte[] certificateData)
		{
			var key = await CertificateLogin(certificateData);
			var response = await _httpClient.PostAsync(_options.SendMessageUri, null);
			
			return response.IsSuccessStatusCode;
		}
	}

	public interface IEdoDocflowService
	{
		
	}
}
