using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using TaxcomEdo.Contracts.Responses;

namespace TaxcomEdo.Contracts.Extensions
{
	public static class TaxcomResponseExtensions
	{
		public static TaxcomResponse ToTaxcomResponse(this HttpResponseMessage responseMessage)
		{
			return responseMessage.IsSuccessStatusCode
				? TaxcomResponse.Success()
				: TaxcomResponse.Error($"Code: {responseMessage.StatusCode} Message: {responseMessage.ReasonPhrase}");
		}
		
		public static async Task<TaxcomResponse<T>> ToTaxcomResponseAsync<T>(
			this HttpResponseMessage responseMessage,
			CancellationToken cancellationToken)
			where T : class
		{
			if(responseMessage.IsSuccessStatusCode)
			{
				var responseString = await responseMessage.Content.ReadAsStringAsync();
				var data = responseString.DeserializeXmlString<T>(false);

				return TaxcomResponse<T>.Success(data);
			}
			
			return TaxcomResponse<T>.Error($"Code: {responseMessage.StatusCode} Message: {responseMessage.ReasonPhrase}");
		}
	}
}
