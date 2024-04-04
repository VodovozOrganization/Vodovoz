using FuelControl.Contracts.Requests;
using FuelControl.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public class GazpromAuthorizationService : IFuelManagmentAuthorizationService
	{
		private const string _authorizationEndpointAddress = "vip/v1/authUser";

		public async Task<string> Login(AuthorizationRequest authorizationRequest)
		{
			var sessionId = string.Empty;

			// Base address is optional but helps in managing relative URIs
			var baseAddress = new Uri(authorizationRequest.BaseAddress);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				var requestData = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("login", authorizationRequest.Login),
					new KeyValuePair<string, string>("password", authorizationRequest.Password)
				};

				HttpContent content = new FormUrlEncodedContent(requestData);
				content.Headers.Add("api_key", authorizationRequest.ApiKey);
				content.Headers.Add("date_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

				try
				{
					var response = await httpClient.PostAsync(_authorizationEndpointAddress, content);

					response.EnsureSuccessStatusCode();

					var responseString = await response.Content.ReadAsStringAsync();

					var responseData = JsonSerializer.Deserialize<AuthorizationResponse>(responseString);

					sessionId = responseData.UserData.SessionId;
				}
				catch(HttpRequestException e)
				{
					Console.WriteLine("Error: " + e.Message);
				}

				return sessionId;
			}
		}
	}
}
