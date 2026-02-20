using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.HealthCheck
{
	public partial class CustomerOrdersApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckRequestForCallController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Создание заявки на звонок",
					checkMethodName => CheckCreateRequestForCall(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckCreateRequestForCall(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("CreateRequestForCall").Get<CreatingRequestForCallDto>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Post,
				$"{_baseAddress}/api/CreateRequestForCall",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.IsSuccess, result.ErrorMessage);
		}
	}
}
