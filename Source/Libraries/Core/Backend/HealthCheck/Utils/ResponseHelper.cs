using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace VodovozHealthCheck.Utils
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

		public static async Task<TResponseDto> PostJsonByUri<TRequestDto, TResponseDto>(string requestUri, IHttpClientFactory httpClientFactory,
			TRequestDto loginRequestDto, string accessToken = null)
		{
			var httpClient = httpClientFactory.CreateClient();

			if(accessToken != null)
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			var responseMessage = await httpClient.PostAsJsonAsync(requestUri, loginRequestDto);

			var response = await responseMessage.Content.ReadFromJsonAsync<TResponseDto>();

			return response;
		}
	}
}
