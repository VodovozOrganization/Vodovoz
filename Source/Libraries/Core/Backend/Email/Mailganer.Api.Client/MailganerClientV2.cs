using Mailganer.Api.Client.Dto;
using Mailganer.Api.Client.Dto.Responses;
using System;
using System.Linq;
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

				if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
				{
					var errorResponse = await response.Content.ReadFromJsonAsync<SendResponse>();
					if(errorResponse != null && errorResponse.Code == "500")
					{
						throw new Exceptions.EmailInStopListException(
							emailMessage.To,
							errorResponse.Message);
					}
				}

				throw new Exception(response.ReasonPhrase);
			}
		}

		public async Task<string> GetEmailBounseMessageInStopList(string email)
		{
			using(var response = await _httpClient.GetAsync($"stop-list/search?email={email}"))
			{
				if(!response.IsSuccessStatusCode)
				{
					throw new HttpRequestException($"Ошибка запроса: {(int)response.StatusCode} {response.ReasonPhrase}");
				}

				var result = await response.Content.ReadFromJsonAsync<StopListSearchResponse>()
					?? throw new InvalidOperationException("Сервис Mailganer вернул пустой ответ");

				if(result.Status != MailganerResponseStatusTypeDto.Ok)
				{
					throw new InvalidOperationException($"Сервис Mailganer вернул ответ с ошибкой: {result.ErrorMessage}");
				}

				var bounceMessage =
					result.Bounces.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.BounceMessage))?.BounceMessage
					?? string.Empty;

				return bounceMessage;
			}
		}

		public async Task RemoveEmailFromStopList(string mailFrom, string email)
		{
			using(var response = await _httpClient.PostAsync($"/stop-list/remove?mail_from={mailFrom}&email={email}", null))
			{
				if(!response.IsSuccessStatusCode)
				{
					throw new HttpRequestException($"Ошибка запроса: {(int)response.StatusCode} {response.ReasonPhrase}");
				}

				var result = await response.Content.ReadFromJsonAsync<MailganerResponseBase>()
					?? throw new InvalidOperationException("Сервис Mailganer вернул пустой ответ");

				if(result.Status != MailganerResponseStatusTypeDto.Ok)
				{
					throw new InvalidOperationException($"Сервис Mailganer вернул ответ с ошибкой: {result.ErrorMessage}");
				}
			}
		}
	}
}
