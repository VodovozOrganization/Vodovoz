using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.DomainModel.UoW;
using RoboatsService.Authentication;
using RoboAtsService.Contracts.Requests;
using RoboAtsService.Contracts.Responses;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Providers;

namespace RoboatsService.HealthCheck
{
	public class RoboatsApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfigurationSection _healthSection;
		private readonly string _baseAddress;
		private readonly string _apiKey;
		private readonly string _apiKeyValue;

		public RoboatsApiHealthCheck(
			ILogger<VodovozHealthCheckBase> logger,
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHealthCheckServiceInfoProvider serviceInfoProvider)
			: base(logger, serviceInfoProvider, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_healthSection = _configuration.GetSection("Health");
			_baseAddress = _healthSection.GetValue<string>("BaseAddress");
			_apiKey = ApiKeyAuthenticationOptions.HeaderName;
			_apiKeyValue = _configuration[_apiKey];
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Регистрация звонка в базе",
					checkMethodName => CheckCallHandler(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение информации о точке доставки",
					checkMethodName => CheckGetDeliveryPointInfo(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение точек доставки по номеру входящего звонка",
					checkMethodName => CheckGetContactPhoneHasOrdersForDeliveryToday(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение телефонов для уточнения времени доставки по контактному телефону сегодняшних заказов",
					checkMethodName => CheckGetCourierPhonesByTodayOrderContactPhone(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetCourierPhonesByTodayOrderContactPhone(string checkMethodName, CancellationToken cancellationToken)
		{
			var getCourierPhonesByTodayOrderContactPhoneSection = _healthSection.GetSection("GetCourierPhonesByTodayOrderContactPhone");
			var phone = getCourierPhonesByTodayOrderContactPhoneSection.GetValue<string>("Phone");

			var result = await HttpResponseHelper.SendRequestAsync<GetCourierPhonesResponse>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetCourierPhonesByTodayOrderContactPhone?counterpartyPhone={phone}",
				_httpClientFactory,
				apiKey: _apiKey,
				apiKeyValue: _apiKeyValue,
				cancellationToken: cancellationToken
				);

			var isHealthy = result.Data?.CallTimeout > 0;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy);
		}

		private async Task<VodovozHealthResultDto> CheckGetContactPhoneHasOrdersForDeliveryToday(string checkMethodName, CancellationToken cancellationToken)
		{
			Order order;
			string phone;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot($"Проверка работоспособности {nameof(CheckGetContactPhoneHasOrdersForDeliveryToday)}"))
			{
				order = unitOfWork.GetAll<Order>().FirstOrDefault(o =>
					o.ContactPhone != null
					&& o.DeliveryDate != null
					&& o.DeliveryDate.Value.Date == DateTime.Today
					);

				phone = $"7{order?.ContactPhone.DigitsNumber}";
			}

			var result = await HttpResponseHelper.SendRequestAsync<GetContactPhoneHasOrdersForDeliveryTodayResponse>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetContactPhoneHasOrdersForDeliveryToday?counterpartyPhone={phone}",
				_httpClientFactory,
				apiKey: _apiKey,
				apiKeyValue: _apiKeyValue,
				cancellationToken: cancellationToken
				);

			var isHealthy = result.Data?.DeliveryPointIds?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckCallHandler(string checkMethodName, CancellationToken cancellationToken)
		{
			var _callHandlerSection = _healthSection.GetSection("CallHandler");

			var requestDto = _healthSection.GetSection("CallHandler").Get<RequestDto>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Get,
				$"{_baseAddress}/api".AppendQueryString(requestDto),
				_httpClientFactory,
				apiKey: _apiKey,
				apiKeyValue: _apiKeyValue,
				cancellationToken: cancellationToken
				);

			var isHealthy = result?.Data == "37";

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result?.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetDeliveryPointInfo(string checkMethodName, CancellationToken cancellationToken)
		{
			var getDeliveryPointInfoSection = _healthSection.GetSection("GetDeliveryPointInfo");
			var deliveryPointId = getDeliveryPointInfoSection.GetValue<int>("DeliveryPointId");
			var streetId = getDeliveryPointInfoSection.GetValue<int>("StreetId");

			var result = await HttpResponseHelper.SendRequestAsync<DeliveryPointInfoResponse>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetDeliveryPointInfo?deliveryPointId={deliveryPointId}",
				_httpClientFactory,
				apiKey: _apiKey,
				apiKeyValue: _apiKeyValue,
				cancellationToken: cancellationToken
				);

			var isHealthy = result.Data?.StreetId == streetId;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
