using Mango.Core.Dto.Vpbx.Requests;
using Mango.Core.Dto.Vpbx.Responses;
using Mango.Core.Sign;
using Mango.Vpbx.Client.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Mango;

namespace Mango.Vpbx.Client.Services
{
	/// <inheritdoc/>
	public class MangoVpbxEmployeesService : IMangoVpbxEmployeesService
	{
		private const string _getUsersEndpoint = "vpbx/config/users/request";
		private const string _createMemberEndpoint = "vpbx/member/create";
		private const string _deleteMemberEndpoint = "vpbx/member/delete";
		private const string _getGroupsEndpoint = "vpbx/groups";
		private const string _updateGroupEndpoint = "vpbx/group/update";

		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			// Незаполненные необязательные параметры не должны попадать в запрос:
			// например, отсутствие extension означает запрос всех сотрудников ВАТС
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

			// Запас на случай, если числовое поле придёт строкой. На практике ВАТС возвращает
			// числа числами, но в документации часть примеров ответа показывает order и wait_sec
			// строками, поэтому разбор допускает оба варианта
			NumberHandling = JsonNumberHandling.AllowReadingFromString,

			// Кириллица в ФИО сотрудника уходит как есть, а не в виде \uXXXX.
			// Требования API это не диктует, экранированный вариант тоже корректен:
			// настройка нужна, чтобы тело запроса читалось в отладочном логе ниже.
			// Ослаблять экранирование здесь безопасно: json уходит значением поля формы,
			// которое кодируется целиком при формировании тела запроса
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		private readonly ILogger<MangoVpbxEmployeesService> _logger;
		private readonly HttpClient _httpClient;
		private readonly ISignGenerator _signGenerator;
		private readonly IMangoSettings _mangoSettings;

		public MangoVpbxEmployeesService(
			ILogger<MangoVpbxEmployeesService> logger,
			HttpClient httpClient,
			ISignGenerator signGenerator,
			IMangoSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_signGenerator = signGenerator ?? throw new ArgumentNullException(nameof(signGenerator));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<VpbxUser>> GetUsersAsync(string extension, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Запрашиваем сотрудников ВАТС Манго{ExtensionPart}",
				string.IsNullOrWhiteSpace(extension) ? string.Empty : $" с внутренним номером {extension}");

			var request = new GetVpbxUsersRequest
			{
				Extension = string.IsNullOrWhiteSpace(extension) ? null : extension
			};

			// Единственный метод API ВАТС, который при успешном выполнении
			// не возвращает код результата, а отдаёт только массив users
			var response = await PostAsync<GetVpbxUsersRequest, GetVpbxUsersResponse>(
				_getUsersEndpoint,
				request,
				false,
				cancellationToken);

			if(response.Users is null)
			{
				// По свободному внутреннему номеру ВАТС отвечает {"users": []},
				// поэтому отсутствие самого массива означает нераспознанный ответ.
				// Трактовать его как "сотрудников нет" нельзя:
				// это привело бы к созданию дубля сотрудника
				throw new MangoVpbxApiException(
					$"ВАТС Манго не вернула список сотрудников на запрос {_getUsersEndpoint}",
					_getUsersEndpoint,
					HttpStatusCode.OK,
					response.Result,
					null);
			}

			_logger.LogInformation("Получено {UsersCount} сотрудников ВАТС Манго", response.Users.Count);

			return response.Users;
		}

		/// <inheritdoc/>
		public async Task<long> CreateMemberAsync(CreateVpbxMemberRequest request, CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			_logger.LogInformation(
				"Создаём сотрудника ВАТС Манго с внутренним номером {Extension}",
				request.Extension);

			var response = await PostAsync<CreateVpbxMemberRequest, CreateVpbxMemberResponse>(
				_createMemberEndpoint,
				request,
				true,
				cancellationToken);

			if(!response.UserId.HasValue)
			{
				throw new MangoVpbxApiException(
					$"ВАТС Манго не вернула id созданного сотрудника с внутренним номером {request.Extension}",
					_createMemberEndpoint,
					HttpStatusCode.OK,
					response.Result,
					null);
			}

			_logger.LogInformation(
				"Создан сотрудник ВАТС Манго {UserId} с внутренним номером {Extension}",
				response.UserId.Value,
				request.Extension);

			return response.UserId.Value;
		}

		/// <inheritdoc/>
		public async Task DeleteMemberAsync(string userId, CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(userId))
			{
				throw new ArgumentException($"{nameof(userId)} не может быть пустым", nameof(userId));
			}

			_logger.LogInformation("Удаляем сотрудника ВАТС Манго {UserId}", userId);

			var request = new DeleteVpbxMemberRequest
			{
				UserId = userId
			};

			await PostAsync<DeleteVpbxMemberRequest, VpbxCommandResponse>(
				_deleteMemberEndpoint,
				request,
				true,
				cancellationToken);

			_logger.LogInformation("Сотрудник ВАТС Манго {UserId} удалён", userId);
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<VpbxGroup>> GetGroupsAsync(string groupId, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Запрашиваем группы ВАТС Манго{GroupIdPart}",
				string.IsNullOrWhiteSpace(groupId) ? string.Empty : $" с идентификатором {groupId}");

			var request = new GetVpbxGroupsRequest
			{
				GroupId = string.IsNullOrWhiteSpace(groupId) ? null : groupId
			};

			var response = await PostAsync<GetVpbxGroupsRequest, GetVpbxGroupsResponse>(
				_getGroupsEndpoint,
				request,
				true,
				cancellationToken);

			if(response.Groups is null)
			{
				// Ответ без массива groups считается нераспознанным. Трактовать его
				// как "групп нет" нельзя: состав группы, полученный этим методом,
				// целиком отправляется обратно при её изменении, поэтому пустой результат
				// привёл бы к удалению всех сотрудников из группы
				throw new MangoVpbxApiException(
					$"ВАТС Манго не вернула список групп на запрос {_getGroupsEndpoint}",
					_getGroupsEndpoint,
					HttpStatusCode.OK,
					response.Result,
					null);
			}

			_logger.LogInformation("Получено {GroupsCount} групп ВАТС Манго", response.Groups.Count);

			return response.Groups;
		}

		/// <inheritdoc/>
		public async Task UpdateGroupOperatorsAsync(
			string groupId,
			IEnumerable<string> operatorIds,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(groupId))
			{
				throw new ArgumentException($"{nameof(groupId)} не может быть пустым", nameof(groupId));
			}

			if(operatorIds is null)
			{
				throw new ArgumentNullException(nameof(operatorIds));
			}

			var operators = operatorIds
				.Select(x => new VpbxGroupOperatorUpdate { Id = x })
				.ToList();

			_logger.LogInformation(
				"Устанавливаем состав группы ВАТС Манго {GroupId}: {OperatorsCount} сотрудников",
				groupId,
				operators.Count);

			var request = new UpdateVpbxGroupRequest
			{
				GroupId = groupId,
				Group = new VpbxGroupUpdate
				{
					Operators = operators
				}
			};

			await PostAsync<UpdateVpbxGroupRequest, VpbxCommandResponse>(
				_updateGroupEndpoint,
				request,
				true,
				cancellationToken);

			_logger.LogInformation("Состав группы ВАТС Манго {GroupId} обновлён", groupId);
		}

		/// <summary>
		/// Выполняет запрос к API ВАТС: подписывает тело запроса, отправляет его
		/// и разбирает ответ, проверяя HTTP-статус и код результата
		/// </summary>
		/// <param name="endpoint">Адрес метода API ВАТС</param>
		/// <param name="request">Тело запроса</param>
		/// <param name="resultCodeRequired">
		/// Возвращает ли метод код результата при успешном выполнении.
		/// Единственный метод, который его не возвращает - запрос списка сотрудников
		/// </param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		private async Task<TResponse> PostAsync<TRequest, TResponse>(
			string endpoint,
			TRequest request,
			bool resultCodeRequired,
			CancellationToken cancellationToken)
			where TResponse : VpbxResponseBase
		{
			// Подпись рассчитывается по той же строке json, которая уходит в запросе,
			// поэтому сериализация выполняется один раз
			var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);
			var sign = _signGenerator.GetSign(_mangoSettings.VpbxApiKey, _mangoSettings.VpbxApiSalt, json);

			_logger.LogDebug("Запрос к {Endpoint} ВАТС Манго: {Json}", endpoint, json);

			var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["vpbx_api_key"] = _mangoSettings.VpbxApiKey,
				["sign"] = sign,
				["json"] = json
			});

			string responseBody;
			HttpStatusCode statusCode;

			using(content)
			using(var response = await _httpClient.PostAsync(endpoint, content, cancellationToken))
			{
				statusCode = response.StatusCode;
				responseBody = await response.Content.ReadAsStringAsync();
			}

			_logger.LogDebug("Ответ {Endpoint} ВАТС Манго: {ResponseBody}", endpoint, responseBody);

			if((int)statusCode < 200 || (int)statusCode > 299)
			{
				throw CreateExceptionForFailedStatusCode(endpoint, statusCode, responseBody);
			}

			TResponse result;
			try
			{
				result = JsonSerializer.Deserialize<TResponse>(responseBody, _jsonSerializerOptions);
			}
			catch(JsonException e)
			{
				throw new MangoVpbxApiException(
					$"Не удалось разобрать ответ ВАТС Манго на запрос {endpoint}: {e.Message}",
					endpoint,
					statusCode,
					null,
					responseBody);
			}

			if(result is null)
			{
				throw new MangoVpbxApiException(
					$"ВАТС Манго вернула пустой ответ на запрос {endpoint}",
					endpoint,
					statusCode,
					null,
					responseBody);
			}

			if(!result.Result.HasValue)
			{
				// Отсутствие кода результата у метода, который его возвращает, означает,
				// что ответ не распознан. Считать такой ответ успешным нельзя: например,
				// для удаления сотрудника это означало бы, что сотрудник остался в ВАТС
				// и продолжает удерживать свой внутренний номер
				if(resultCodeRequired)
				{
					throw new MangoVpbxApiException(
						$"ВАТС Манго не вернула код результата на запрос {endpoint}",
						endpoint,
						statusCode,
						null,
						responseBody);
				}

				return result;
			}

			if(result.Result.Value != VpbxResultCodes.Success)
			{
				throw new MangoVpbxApiException(
					$"Запрос {endpoint} к ВАТС Манго завершился с кодом результата {result.Result.Value}"
						+ GetErrorDetails(result),
					endpoint,
					statusCode,
					result.Result,
					responseBody);
			}

			return result;
		}

		/// <summary>
		/// Собирает пояснение к коду результата: описание ошибки и параметры запроса,
		/// к которым она относится. Оба поля заполняются не всегда
		/// </summary>
		private static string GetErrorDetails(VpbxResponseBase response)
		{
			var details = new List<string>();

			if(!string.IsNullOrWhiteSpace(response.Description))
			{
				details.Add(response.Description);
			}

			if(response.Fields.HasValue)
			{
				details.Add($"параметры запроса: {response.Fields.Value.GetRawText()}");
			}

			return details.Count == 0 ? string.Empty : $": {string.Join(", ", details)}";
		}

		/// <summary>
		/// Формирует исключение по ответу с неуспешным HTTP-статусом.
		/// При превышении лимита количества запросов ВАТС отдаёт тело в отдельном формате,
		/// без кода результата, поэтому оно разбирается отдельно
		/// </summary>
		private static MangoVpbxApiException CreateExceptionForFailedStatusCode(
			string endpoint,
			HttpStatusCode statusCode,
			string responseBody)
		{
			if((int)statusCode == 429)
			{
				var error = TryDeserialize<VpbxErrorResponse>(responseBody);

				var errorMessage = string.IsNullOrWhiteSpace(error?.Message)
					? "Rate limit exceeded."
					: error.Message;

				return new MangoVpbxApiException(
					$"Запрос {endpoint} отклонён ВАТС Манго из-за превышения лимита количества запросов: {errorMessage}",
					endpoint,
					statusCode,
					VpbxResultCodes.RateLimitExceeded,
					responseBody);
			}

			return new MangoVpbxApiException(
				$"Запрос {endpoint} к ВАТС Манго завершился с HTTP-статусом {(int)statusCode}",
				endpoint,
				statusCode,
				TryDeserialize<VpbxCommandResponse>(responseBody)?.Result,
				responseBody);
		}

		/// <summary>
		/// Разбирает тело ответа, не бросая исключений: тело неуспешного ответа
		/// не обязано быть корректным json
		/// </summary>
		private static T TryDeserialize<T>(string responseBody)
			where T : class
		{
			if(string.IsNullOrWhiteSpace(responseBody))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<T>(responseBody, _jsonSerializerOptions);
			}
			catch(JsonException)
			{
				return null;
			}
		}
	}
}
