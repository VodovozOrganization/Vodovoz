using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoLoginService
	{
		protected EdoLoginService(IEdoAuthorizationService edoAuthorizationService)
		{
			EdoAuthorizationService = edoAuthorizationService ?? throw new ArgumentNullException(nameof(edoAuthorizationService));
		}
		
		protected IEdoAuthorizationService EdoAuthorizationService { get; }
		
		protected async Task<string> CertificateLoginAsync(byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var key = await EdoAuthorizationService.CertificateLoginAsync(certificateData, cancellationToken);
			return key;
		}
		
		protected async Task<string> LoginAsync(
			string login,
			string password,
			HttpRequestType requestType = HttpRequestType.Post,
			CancellationToken cancellationToken = default)
		{
			var key = await EdoAuthorizationService.LoginAsync(login, password, requestType, cancellationToken);
			return key;
		}
		
		protected HttpRequestMessage PrepareGetHttpRequestMessage(string assistantKey, string uri)
		{
			var message = new HttpRequestMessage(HttpMethod.Get, uri);
			message.Headers.Add(ExternalApiConstants.AssistantKeyHeader, assistantKey);
			return message;
		}
		
		protected ByteArrayContent PrepareByteArrayContent(byte[] data, string assistantKey)
		{
			var content = new ByteArrayContent(data);
			content.Headers.Add(ExternalApiConstants.AssistantKeyHeader, assistantKey);
			content.Headers.ContentLength = data.Length;
			return content;
		}
	}
}
