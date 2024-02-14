using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace VodovozHealthCheck.Helpers
{
	public class ResponseHelper
	{
		public static async Task<T> GetJsonByUri<T>(string requestUri, IHttpClientFactory httpClientFactory, string accessToken = null)
		{
			var httpClient = httpClientFactory.CreateClient();

			if(accessToken != null)
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			var responseMessage = await httpClient.GetFromJsonAsync(requestUri, typeof(T));

			return (T)responseMessage;
		}

		public static async Task<object> GetByUri(string requestUri, IHttpClientFactory httpClientFactory, string accessToken = null)
		{
			var httpClient = httpClientFactory.CreateClient();

			if(accessToken != null)
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			var responseMessage = await httpClient.GetAsync(requestUri);

			return responseMessage;
		}

		public static async Task<TResponseDto> PostJsonByUri<TRequestDto, TResponseDto>(string requestUri, IHttpClientFactory httpClientFactory,
			TRequestDto requestDto, string accessToken = null)
		{
			var httpClient = httpClientFactory.CreateClient();

			if(accessToken != null)
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			var responseMessage = await httpClient.PostAsJsonAsync(requestUri, requestDto);

			var response = await responseMessage.Content.ReadFromJsonAsync<TResponseDto>();

			return response;
		}

		public static bool CheckUriExists(string uri)
		{
			try
			{
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
				webRequest.Timeout = 30000;
				webRequest.Method = "GET";

				HttpWebResponse response = null;

				try
				{
					response = (HttpWebResponse)webRequest.GetResponse();

					int statusCode = (int)response.StatusCode;
					if(statusCode >= 100 && statusCode < 400) //Good requests
					{
						return true;
					}
					else if(statusCode >= 500 && statusCode <= 510) //Server Errors
					{
						return false;
					}
				}
				catch(WebException webException)
				{
					return false;
				}
				finally
				{
					if(response != null)
					{
						response.Close();
					}
				}
			}
			catch(Exception ex)
			{
				return false;
			}

			return false;
		}
	}
}
