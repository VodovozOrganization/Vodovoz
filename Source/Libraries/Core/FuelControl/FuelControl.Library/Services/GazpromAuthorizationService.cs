using FuelControl.Contracts.Responses;
using FuelControl.Library.Services.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Vodovoz.Settings.Fuel;

namespace FuelControl.Library.Services
{
	public class GazpromAuthorizationService : IFuelManagmentAuthorizationService
	{
		private const string _authorizationEndpointAddress = "vip/v1/authUser";

		private readonly ILogger<GazpromAuthorizationService> _logger;
		private readonly IFuelControlSettings _fuelControlSettings;

		public GazpromAuthorizationService(ILogger<GazpromAuthorizationService> logger, IFuelControlSettings fuelControlSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<string> Login(string login, string password, string apiKey)
		{
			if(string.IsNullOrWhiteSpace(login))
			{
				throw new ArgumentException($"'{nameof(login)}' cannot be null or whitespace.", nameof(login));
			}

			if(string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentException($"'{nameof(password)}' cannot be null or whitespace.", nameof(password));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);
			var httpContent = CreateHttpContent(login, password, apiKey);

			_logger.LogDebug("Выполняется запрос авторизации пользователя {UserLogin} с паролем {UserPassword} ключ API {ApiKey}",
				login,
				password,
				apiKey);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				var response = await httpClient.PostAsync(_authorizationEndpointAddress, httpContent);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<AuthorizationResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessages =
						$"На запрос авторизации вернулся ответ с ошибками: {string.Concat(responseData.Status.Errors.Select(e => e.Message))}";

					_logger.LogError(errorMessages);

					throw new FuelControlException(errorMessages);
				}

				var sessionId = responseData.UserData.SessionId;

				_logger.LogDebug("Авторизация выполнена успешно. ID сессии {SessionId} получено {NowDateTime}",
				sessionId,
				DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss"));

				return sessionId;
			}
		}

		private HttpContent CreateHttpContent(string login, string password, string apiKey)
		{
			var requestData = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("login", login),
				new KeyValuePair<string, string>("password", password)
			};

			var content = new FormUrlEncodedContent(requestData);
			content.Headers.Add("api_key", apiKey);
			content.Headers.Add("date_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

			return content;
		}
	}
}
