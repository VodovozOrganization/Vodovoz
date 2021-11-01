using ApiClientProvider;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mailjet.Api.Abstractions.Events;

namespace MailjetDebugAPI.Endpoints
{
	public class EventsRecieverEndpoint
	{
		private const string _eventCallbackEndpointParameter = "EventCallbackPath";

		private readonly IApiClientProvider _apiHelper;
		private readonly string _eventCallbackEndpointPath;

		public EventsRecieverEndpoint(IConfigurationSection section, IApiClientProvider apiHelper)
		{
			if(section is null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			_eventCallbackEndpointPath = section.GetValue(_eventCallbackEndpointParameter, "EventCallback");

			_apiHelper = apiHelper ?? throw new ArgumentNullException(nameof(apiHelper));
		}

		public async Task<string> SendEvent<T>(T mailEvent) where T : MailEvent
		{
			using(HttpResponseMessage response = await _apiHelper.Client.PostAsJsonAsync(_eventCallbackEndpointPath, mailEvent))
			{
				if(!response.IsSuccessStatusCode)
				{
					return response.ReasonPhrase;
				}

				return "Event succesfully sended";
			}
		}
	}
}
