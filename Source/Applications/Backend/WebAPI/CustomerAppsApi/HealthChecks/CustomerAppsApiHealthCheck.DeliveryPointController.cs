using CustomerAppsApi.Library.Dto;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckDeliveryPointController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Получение списка ТД",
					checkMethodName => CheckGetDeliveryPoints(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Создание ТД",
					checkMethodName => CheckAddDeliveryPoint(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Обновление комментария к ТД из ИПЗ",
					checkMethodName => CheckUpdateOnlineComment(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetDeliveryPoints(string checkMethodName, CancellationToken cancellationToken)
		{
			var source = _healthSection.GetSection("GetDeliveryPointsSource").Get<string>();
			var counterpartyErpId = _healthSection.GetSection("GetDeliveryPointsCounterpartyErpId").Get<int>();

			var result = await HttpResponseHelper.SendRequestAsync<DeliveryPointsDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetDeliveryPoints?source={source}&counterpartyErpId={counterpartyErpId}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data.DeliveryPointsInfo?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.Data?.ErrorDescription ?? result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckAddDeliveryPoint(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("AddDeliveryPoint").Get<NewDeliveryPointInfoDto>();

			var result = await HttpResponseHelper.SendRequestAsync<CreatedDeliveryPointDto>(
				HttpMethod.Post,
				$"{_baseAddress}/api/AddDeliveryPoint",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.StatusCode == HttpStatusCode.Created;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckUpdateOnlineComment(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("UpdateOnlineComment").Get<UpdatingDeliveryPointCommentDto>();

			var result = await HttpResponseHelper.SendRequestAsync<object>(
				HttpMethod.Post,
				$"{_baseAddress}/api/UpdateOnlineComment",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.IsSuccess, result.ErrorMessage);
		}


	}
}
