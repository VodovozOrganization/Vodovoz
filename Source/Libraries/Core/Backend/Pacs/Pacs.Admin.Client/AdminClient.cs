using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using Pacs.Server;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Admin.Client
{
	public class AdminClient
    {
		private readonly string _settingsUrl = "pacs/settings";
		private readonly string _adminCommandsUrl = "pacs/admin/operator";
		private readonly HttpClient _httpClient = new HttpClient();
		private readonly ILogger<AdminClient> _logger;
		private readonly IPacsAdministratorProvider _adminProvider;
		private readonly IPacsSettings _pacsSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public AdminClient(ILogger<AdminClient> logger, IPacsAdministratorProvider adminProvider, IPacsSettings pacsSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_adminProvider = adminProvider ?? throw new ArgumentNullException(nameof(adminProvider));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

			if(_adminProvider.AdministratorId.HasValue)
			{
				throw new PacsInitException("Апи клиент администратора СКУД недоступен, так как пользователь не является администратором");
			}
		}

		public async Task SetSettings(DomainSettings settings)
		{
			var uri = $"{_pacsSettings.AdministratorApiUrl}/{_settingsUrl}/set";
			var content = new StringContent(JsonSerializer.Serialize(settings), Encoding.UTF8, "application/json");
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.AdministratorApiKey);

			try
			{
				await _httpClient.PostAsync(uri, content);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при установке новых настроек");
				throw;
			}
		}

		public async Task<DomainSettings> GetSettings()
		{
			var uri = $"{_pacsSettings.AdministratorApiUrl}/{_settingsUrl}/get";
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.AdministratorApiKey);

			try
			{
				var response = await _httpClient.GetAsync(uri);
				if(response.IsSuccessStatusCode)
				{
					var responseBody = await response.Content.ReadAsStringAsync();
					var registrationResult = JsonSerializer.Deserialize<DomainSettings>(responseBody, _jsonSerializerOptions);
					return registrationResult;
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

		public async Task<OperatorStateEvent> StartBreak(int operatorId, string reason, OperatorBreakType breakType, 
			CancellationToken cancellationToken = default)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_adminCommandsUrl}/startbreak";
			var payload = new AdminStartBreak
			{
				OperatorId = operatorId,
				BreakType = breakType,
				AdminId = _adminProvider.AdministratorId.Value,
				Reason = reason
			};
			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.PostAsync(uri, content);
				var responseContent = await response.Content.ReadAsStringAsync();
				var operatorResult = JsonSerializer.Deserialize<OperatorResult>(responseContent, _jsonSerializerOptions);
				if(operatorResult.Result == Result.Success)
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
				_logger.LogError(ex, "Ошибка при выводе администратором на перерыв оператора {OperatorId}", operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> EndBreak(int operatorId, string reason, CancellationToken cancellationToken = default)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_adminCommandsUrl}/endbreak";
			var payload = new AdminEndBreak 
			{ 
				OperatorId = operatorId,
				AdminId = _adminProvider.AdministratorId.Value,
				Reason = reason
			};
			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.PostAsync(uri, content);
				var responseContent = await response.Content.ReadAsStringAsync();
				var operatorResult = JsonSerializer.Deserialize<OperatorResult>(responseContent, _jsonSerializerOptions);
				if(operatorResult.Result == Result.Success)
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
