using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Admin.Client
{
	public class AdminClient : IAdminClient
	{
		private readonly string _settingsUrl = "pacs/settings";
		private readonly string _adminCommandsUrl = "pacs/admin/operator";
		private readonly HttpClient _httpClient;
		private readonly ILogger<AdminClient> _logger;
		private readonly IPacsAdministratorProvider _adminProvider;
		private readonly IPacsSettings _pacsSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly Dictionary<string, string> _endpointsUrl;

		public AdminClient(
			ILogger<AdminClient> logger,
			IPacsAdministratorProvider adminProvider,
			IPacsSettings pacsSettings,
			HttpClient httpClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_adminProvider = adminProvider ?? throw new ArgumentNullException(nameof(adminProvider));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

			if(_adminProvider.AdministratorId == null)
			{
				throw new PacsInitException("Апи клиент администратора СКУД недоступен, так как пользователь не является администратором");
			}

			_endpointsUrl = new Dictionary<string, string>
			{
				{ nameof(SetSettings), $"{_pacsSettings.AdministratorApiUrl}/{_settingsUrl}/set" },
				{ nameof(GetSettings), $"{_pacsSettings.AdministratorApiUrl}/{_settingsUrl}/get" },
				{ nameof(StartBreak), $"{_pacsSettings.OperatorApiUrl}/{_adminCommandsUrl}/startbreak" },
				{ nameof(EndBreak), $"{_pacsSettings.OperatorApiUrl}/{_adminCommandsUrl}/endbreak" },
				{ nameof(EndWorkShift), $"{_pacsSettings.OperatorApiUrl}/{_adminCommandsUrl}/endworkshift" }
			};
		}

		public async Task SetSettings(DomainSettings settings)
		{
			try
			{
				await _httpClient.PostAsJsonAsync(
					_endpointsUrl[nameof(SetSettings)],
					settings,
					new Dictionary<string, string>
					{
						{ "ApiKey", _pacsSettings.AdministratorApiKey },
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при установке новых настроек");
				throw;
			}
		}

		public async Task<DomainSettings> GetSettings()
		{
			try
			{
				var response = await _httpClient.GetAsync(
					_endpointsUrl[nameof(GetSettings)],
					new Dictionary<string, string>
					{
						{ "ApiKey", _pacsSettings.AdministratorApiKey },
					});

				if(response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<DomainSettings>(_jsonSerializerOptions);
				}
				else
				{
					throw new InvalidOperationException($"Code: {response.StatusCode}. {response.ReasonPhrase}");
				}

			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при установке новых настроек");
				throw;
			}
		}

		public async Task<OperatorStateEvent> StartBreak(
			int operatorId,
			string reason,
			OperatorBreakType breakType,
			CancellationToken cancellationToken = default)
		{
			var payload = new AdminStartBreak
			{
				EventId = Guid.NewGuid(),
				OperatorId = operatorId,
				BreakType = breakType,
				AdminId = _adminProvider.AdministratorId.Value,
				Reason = reason
			};

			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					_endpointsUrl[nameof(StartBreak)],
					payload,
					new Dictionary<string, string>
					{
						{ "ApiKey",  _pacsSettings.OperatorApiKey }
					});

				if(response.IsSuccessStatusCode)
				{
					var operatorResult = await response.Content.ReadFromJsonAsync<OperatorResult>(_jsonSerializerOptions);

					if(operatorResult.Result == Result.Success)
					{
						return operatorResult.OperatorState;
					}

					throw new PacsException(operatorResult.FailureDescription);
				}
				throw new PacsException($"Не удалось начать перерыв, {response.StatusCode}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выводе администратором на перерыв оператора {OperatorId}", operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> EndBreak(int operatorId, string reason, CancellationToken cancellationToken = default)
		{
			var payload = new AdminEndBreak
			{
				EventId = Guid.NewGuid(),
				OperatorId = operatorId,
				AdminId = _adminProvider.AdministratorId.Value,
				Reason = reason
			};

			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					_endpointsUrl[nameof(EndBreak)],
					payload,
					new Dictionary<string, string>
					{
						{ "ApiKey",  _pacsSettings.OperatorApiKey }
					});

				var operatorResult = await response.Content.ReadFromJsonAsync<OperatorResult>(_jsonSerializerOptions);

				if(response.IsSuccessStatusCode && operatorResult.Result == Result.Success)
				{
					return operatorResult.OperatorState;
				}
				else
				{
					throw new PacsException(operatorResult.FailureDescription);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выводе администратором с перерыва оператора {OperatorId}", operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> EndWorkShift(int operatorId, string reason, CancellationToken cancellationToken = default)
		{
			var payload = new AdminEndWorkShift
			{
				EventId = Guid.NewGuid(),
				OperatorId = operatorId,
				AdminId = _adminProvider.AdministratorId.Value,
				Reason = reason
			};

			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					_endpointsUrl[nameof(EndWorkShift)],
					payload,
					new Dictionary<string, string>
					{
						{ "ApiKey",  _pacsSettings.OperatorApiKey }
					});

				var operatorResult = await response.Content.ReadFromJsonAsync<OperatorResult>(_jsonSerializerOptions);

				if(response.IsSuccessStatusCode && operatorResult.Result == Result.Success)
				{
					return operatorResult.OperatorState;
				}
				else
				{
					throw new PacsException(operatorResult.FailureDescription);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выводе администратором с перерыва оператора {OperatorId}", operatorId);
				throw;
			}
		}
	}
}
