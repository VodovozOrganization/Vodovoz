using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using VodovozInfrastructure.Endpoints;

namespace TaxcomEdo.Client
{
	public static class HttpResponseMessageExtensions
	{
		public static async Task<Error> ToTaxcomError(
			this HttpResponseMessage response,
			JsonSerializerOptions jsonSerializerOptions,
			CancellationToken cancellationToken = default)
		{
			var problemDetails = await response.Content
				.ReadFromJsonAsync<ProblemDetailResponse>(jsonSerializerOptions, cancellationToken);

			var errorCode = problemDetails?.Type ?? $"TaxcomHttp{(int)response.StatusCode}";
			var errorMessage = problemDetails?.Detail
				?? $"Ошибка при обращении к Taxcom Edo Api, HTTP {(int)response.StatusCode}";

			return new Error(errorCode, errorMessage);
		}
	}
}
