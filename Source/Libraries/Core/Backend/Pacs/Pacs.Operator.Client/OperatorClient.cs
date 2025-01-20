using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client.Consumers;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Operators.Client
{
	public class OperatorClient : IOperatorClient, IObserver<OperatorStateEvent>, IDisposable
	{
		private readonly string _url = "pacs/operator";
		private readonly HttpClient _httpClient = new HttpClient();
		private readonly IDisposable _stateSubscription;
		private readonly int? _operatorId;
		private readonly ILogger<OperatorClient> _logger;
		private readonly IPacsSettings _pacsSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public int? OperatorId => _operatorId;

		public OperatorClient(
			ILogger<OperatorClient> logger,
			IPacsOperatorProvider operatorProvider,
			IPacsSettings pacsSettings,
			OperatorStateConsumer operatorStateConsumer)
		{
			if(operatorProvider is null)
			{
				throw new ArgumentNullException(nameof(operatorProvider));
			}

			if(operatorStateConsumer is null)
			{
				throw new ArgumentNullException(nameof(operatorStateConsumer));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorId = operatorProvider.OperatorId;
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));

			_jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
			_stateSubscription = operatorStateConsumer.Subscribe(this);
		}

		public event EventHandler<OperatorStateEvent> StateChanged;

		public async Task<OperatorStateEvent> StartWorkShift(string phoneNumber)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/startworkshift";
			var payload = new StartWorkShift
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value,
				PhoneNumber = phoneNumber
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
				_logger.LogError(ex, "Ошибка при начале новой смены оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> EndWorkShift(string reason = null)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/endworkshift";

			var payload = new EndWorkShift
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value
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
				_logger.LogError(ex, "Ошибка при завершении смены оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> ChangeNumber(string phoneNumber)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/changephone";
			var payload = new ChangePhone
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value,
				PhoneNumber = phoneNumber
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
				_logger.LogError(ex, "Ошибка при смене номера телефона оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> StartBreak(OperatorBreakType breakType, CancellationToken cancellationToken = default)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/startbreak";
			var payload = new StartBreak
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value, 
				BreakType = breakType 
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
				_logger.LogError(ex, "Ошибка при уходе на перерыв оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> EndBreak(CancellationToken cancellationToken = default)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/endbreak";
			var payload = new EndBreak
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value
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
					_logger.LogError("Error: {FailureDescription}, OperatorState: {OperatorStateState}, {@OperatorState}",
						operatorResult.FailureDescription,
						operatorResult.OperatorState?.State,
						operatorResult.OperatorState);

					throw new PacsException(operatorResult.FailureDescription);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выходе с перерыва оператора {OperatorId}", _operatorId);
				throw;
			}
		}
		
		public async Task<OperatorStateEvent> Connect(CancellationToken cancellationToken = default)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/connect";

			var payload = new Connect
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value
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
				_logger.LogError(ex, "Ошибка при подключении оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<OperatorStateEvent> Disconnect(CancellationToken cancellationToken = default)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/disconnect";

			var payload = new Disconnect
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value
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
				_logger.LogError(ex, "Ошибка при отключении оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task KeepAlive(CancellationToken cancellationToken = default)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/keep_alive";
			var payload = new KeepAlive
			{
				EventId = Guid.NewGuid(),
				OperatorId = _operatorId.Value
			};

			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.PostAsync(uri, content);
				if(!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("Отправка KeepAlive сообщения оператора {OperatorId} была не успешна. Код {HttpCode}", 
						_operatorId, (int)response.StatusCode);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке KeepAlive сообщения оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<GlobalBreakAvailabilityEvent> GetGlobalBreakAvailability()
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/pacs/global-break-availability/get";
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.GetAsync(uri);
				var responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<GlobalBreakAvailabilityEvent>(responseContent, _jsonSerializerOptions);
				return result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении состояния возможности перерыва");
				throw;
			}
		}

		public async Task<OperatorsOnBreakEvent> GetOperatorsOnBreak()
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/pacs/global-break-availability/get-operators-on-break";
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.GetAsync(uri);
				var responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<OperatorsOnBreakEvent>(responseContent, _jsonSerializerOptions);
				return result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении состояния возможности перерыва");
				throw;
			}
		}

		public async Task<OperatorBreakAvailability> GetOperatorBreakAvailability(int operatorId)
		{
			Validate();
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/break-availability?orepatorId={operatorId}";
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.GetAsync(uri);
				var responseContent = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<OperatorBreakAvailability>(responseContent, _jsonSerializerOptions);
				return await Task.FromResult(result);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении состояния возможности перерыва оператора");
				throw;
			}
		}

		private void Validate()
		{
			if(_operatorId == null)
			{
				throw new PacsInitException("Попытка использовать клиент оператора пользователем не являющимся оператором ");
			}
		}

		public void OnNext(OperatorStateEvent value)
		{
			StateChanged?.Invoke(this, value);
		}

		public void OnError(Exception error)
		{
			_logger.LogError(error, "Ошибка при передачи на стороне клиента полученного уведомления о состоянии оператора");
		}

		public void OnCompleted()
		{
			_stateSubscription?.Dispose();
		}

		public void Dispose()
		{
			OnCompleted();
		}
	}
}
