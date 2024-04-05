using FuelControl.Contracts.Requests;
using FuelControl.Contracts.Responses;
using FuelControl.Library.Services.Exceptions;
using Microsoft.Extensions.Logging;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public class GazpromAuthorizationService : IFuelManagmentAuthorizationService
	{
		private const string _authorizationEndpointAddress = "vip/v1/authUser";

		private readonly ILogger<GazpromAuthorizationService> _logger;

		public GazpromAuthorizationService(ILogger<GazpromAuthorizationService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<string> Login(AuthorizationRequest authorizationRequest)
		{

			var baseAddress = CreateBaseAddressUri(authorizationRequest);
			var httpContent = CreateHttpContent(authorizationRequest);

			_logger.LogDebug("Выполняется запрос авторизации пользователя {UserLogin} с паролем {UserPassword} ключ API {ApiKey}",
				authorizationRequest.Login,
				authorizationRequest.Password,
				authorizationRequest.ApiKey);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				try
				{
					var response = await httpClient.PostAsync(_authorizationEndpointAddress, httpContent);

					var responseString = await response.Content.ReadAsStringAsync();

					var responseData = JsonSerializer.Deserialize<AuthorizationResponse>(responseString);

					if(!response.IsSuccessStatusCode)
					{
						var errorMessage =
							responseData.Status.Errors != null && responseData.Status.Errors.Count() > 0
							? string.Join("; ", responseData.Status.Errors.Select(e => e.Message).ToArray())
							: string.Empty;

						throw new FuelControlAuthorizationException($"Ошибка выполнения запроса авторизации: {errorMessage}");
					}

					var sessionId = responseData.UserData.SessionId;

					_logger.LogDebug("Авторизация выполнена успешно. ID сессии {SessionId} получено {NowDateTime}",
					sessionId,
					DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss"));

					return sessionId;
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		private Uri CreateBaseAddressUri(AuthorizationRequest authorizationRequest)
		{
			return new Uri(authorizationRequest.BaseAddress);
		}

		private HttpContent CreateHttpContent(AuthorizationRequest authorizationRequest)
		{
			var requestData = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("login", authorizationRequest.Login),
					new KeyValuePair<string, string>("password", authorizationRequest.Password)
				};

			var content = new FormUrlEncodedContent(requestData);
			content.Headers.Add("api_key", authorizationRequest.ApiKey);
			content.Headers.Add("date_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

			return content;
		}
	}
}
