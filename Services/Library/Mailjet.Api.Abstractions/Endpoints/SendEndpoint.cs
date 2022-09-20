using ApiClientProvider;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mailjet.Api.Abstractions.Endpoints
{
	public class SendEndpoint
	{
		private IApiClientProvider _apiHelper;
		private readonly string _sendEndpointPath = "send";

		public SendEndpoint(IApiClientProvider apiHelper)
		{
			_apiHelper = apiHelper;
		}

		public async Task<SendResponse> Send(SendPayload payload)
		{
			using(HttpResponseMessage response = await _apiHelper.Client.PostAsJsonAsync(_sendEndpointPath, payload))
			{
				if(response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadAsAsync<SendResponse>();
					return result;
				}

				throw new Exception(response.ReasonPhrase);
			}
		}
	}
}
