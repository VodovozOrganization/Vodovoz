using Mailganer.Api.Client.Dto;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mailganer.Api.Client
{
	public class MailganerClientV2
	{
		private readonly HttpClient _httpClient;

		public MailganerClientV2(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public async Task<SendResponse> Send(EmailMessage emailMessage)
		{
			var json = JsonSerializer.Serialize(emailMessage);
			var content = new StringContent(json);

			// сделано специально так, потому что сервер не принимает автоматически 
			// сгенерированный заголовок application/json; charset=utf-8
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using(var response = await _httpClient.PostAsync("mail/send", content))
			{
				if(response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<SendResponse>();
					return result;
				}

				throw new Exception(response.ReasonPhrase);
			}
		}
	}
}
