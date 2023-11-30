using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Commands;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Operator.Client
{
	public class OperatorClient : IOperatorClient, IObserver<OperatorState>, IDisposable
	{
		private readonly string _url = "pacs/operator";
		private readonly HttpClient _httpClient = new HttpClient();
		private readonly IDisposable _stateSubscription;
		private readonly int _operatorId;
		private readonly ILogger<OperatorClient> _logger;
		private readonly IPacsSettings _pacsSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public int OperatorId => throw new NotImplementedException();

		public OperatorClient(
			int operatorId,
			ILogger<OperatorClient> logger,
			IPacsSettings pacsSettings,
			OperatorStateConsumer operatorStateConsumer)
		{
			if(operatorStateConsumer is null)
			{
				throw new ArgumentNullException(nameof(operatorStateConsumer));
			}

			_operatorId = operatorId;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));

			_jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
			_stateSubscription = operatorStateConsumer.Subscribe(this);
		}

		public event EventHandler<OperatorState> StateChanged;

		public async Task<OperatorState> GetState()
		{
			throw new NotImplementedException();
		}

		public async Task<OperatorState> StartWorkShift(string phoneNumber)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/startworkshift";
			var payload = new StartWorkShift { OperatorId = _operatorId, PhoneNumber = phoneNumber };
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

		public async Task<OperatorState> EndWorkShift()
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/endworkshift";
			var payload = new EndWorkShift { OperatorId = _operatorId };
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

		public async Task<OperatorState> ChangeNumber(string phoneNumber)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/changephone";
			var payload = new ChangePhone { OperatorId = _operatorId, PhoneNumber = phoneNumber };
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

		public async Task<OperatorState> StartBreak(CancellationToken cancellationToken = default)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/startbreak";
			var payload = new StartBreak { OperatorId = _operatorId };
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

		public async Task<OperatorState> EndBreak(CancellationToken cancellationToken = default)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/endbreak";
			var payload = new EndBreak { OperatorId = _operatorId };
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
				_logger.LogError(ex, "Ошибка при выходе с перерыва оператора {OperatorId}", _operatorId);
				throw;
			}
		}

		public async Task<OperatorState> Connect(CancellationToken cancellationToken = default)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/connect";
			var payload = new Connect { OperatorId = _operatorId };
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

		public async Task<OperatorState> Disconnect(CancellationToken cancellationToken = default)
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/{_url}/disconnect";
			var payload = new Disconnect { OperatorId = _operatorId };
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

		public async Task<bool> GetBreakAvailability()
		{
			var uri = $"{_pacsSettings.OperatorApiUrl}/pacs/break-availability/get";
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.OperatorApiKey);

			try
			{
				var response = await _httpClient.GetAsync(uri);
				var responseContent = await response.Content.ReadAsStringAsync();
				return bool.Parse(responseContent);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении состояния возможности перерыва");
				throw;
			}
		}

		public void OnNext(OperatorState value)
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
