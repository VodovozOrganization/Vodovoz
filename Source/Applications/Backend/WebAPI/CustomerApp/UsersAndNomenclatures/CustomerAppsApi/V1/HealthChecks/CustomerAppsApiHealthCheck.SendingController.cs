using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V1.Dto;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V1.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckSendingController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Отправка кода авторизации на указанный email",
					checkMethodName => CheckSendCodeToEmail(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckSendCodeToEmail(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestBody = _healthSection.GetSection("SendCodeToEmail").Get<SendingCodeToEmailDto>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Post,
				$"{_baseAddress}/api/SendCodeToEmail",
				_httpClientFactory,
				requestBody.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.IsSuccess, result.ErrorMessage);
		}
	}
}
