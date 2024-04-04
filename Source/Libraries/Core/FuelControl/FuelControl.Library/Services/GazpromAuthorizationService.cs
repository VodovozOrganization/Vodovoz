using FuelControl.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public class GazpromAuthorizationService
	{
		private readonly string _baseAddress;

		public GazpromAuthorizationService(string baseAddress)
		{
			_baseAddress = baseAddress;
		}

		public async Task<string> Login(AuthorizationRequest authorizationRequest)
		{
			// Base address is optional but helps in managing relative URIs
			var baseAddress = new Uri(_baseAddress);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				// Form data is typically sent as key-value pairs
				var formData = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("login", $"{authorizationRequest.Login}"),
					new KeyValuePair<string, string>("password", $"{authorizationRequest.Password}")
				};
				
				// Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
				HttpContent content = new FormUrlEncodedContent(formData);

				try
				{
					// Send a POST request to the specified Uri as an asynchronous operation.
					HttpResponseMessage response = await httpClient.PostAsync("vip/v1/authUser", content);

					// Ensure we get a successful response.
					response.EnsureSuccessStatusCode();

					// Read the response as a string.
					string result = await response.Content.ReadAsStringAsync();
					Console.WriteLine(result);
				}
				catch(HttpRequestException e)
				{
					Console.WriteLine("Error: " + e.Message);
				}

				return string.Empty;
			}
		}
	}
}
