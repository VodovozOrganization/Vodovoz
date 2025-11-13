using Mailganer.Api.Client.Dto;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mailganer.Api.Client
{
	public class MailganerClientV1
	{
		private readonly HttpClient _httpClient;
		private readonly IOptions<MailganerSettings> _mailganerSettings;

		public MailganerClientV1(HttpClient httpClient, IOptions<MailganerSettings> mailganerSettings)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_mailganerSettings = mailganerSettings ?? throw new ArgumentNullException(nameof(mailganerSettings));
		}

		public async Task<PackageSendResponse> PackageSend(PackageEmailMessage emailMessage)
		{
			var key = _mailganerSettings.Value.ApiKey;
			var url = $"add_json_package?key={key}";

			var json = JsonSerializer.Serialize(emailMessage);
			var content = new StringContent(json);

			// сделано специально так, потому что сервер не принимает автоматически 
			// сгенерированный заголовок application/json; charset=utf-8
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using(var response = await _httpClient.PostAsync(url, content))
			{
				if(response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<PackageSendResponse>();
					return result;
				}

				throw new Exception(response.ReasonPhrase);
			}
		}
	}
}
