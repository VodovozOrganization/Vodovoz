using Mango.Core.Dto.Vpbx.Requests;
using Mango.Core.Dto.Vpbx.Responses;
using Mango.Vpbx.Client.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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

		private readonly ILogger<MangoVpbxEmployeesService> _logger;
		private readonly IMangoVpbxApiClient _apiClient;

		public MangoVpbxEmployeesService(
			ILogger<MangoVpbxEmployeesService> logger,
			IMangoVpbxApiClient apiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<VpbxUser>> GetAllUsersAsync(CancellationToken cancellationToken)
		{
			return await GetUsersAsync(null, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<VpbxUser>> GetUserAsync(long extension, CancellationToken cancellationToken)
		{
			return await GetUsersAsync(extension.ToString(), cancellationToken);
		}

		/// <inheritdoc/>
		private async Task<IReadOnlyList<VpbxUser>> GetUsersAsync(string extension, CancellationToken cancellationToken)
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
			var response = await _apiClient.PostAsync<GetVpbxUsersRequest, GetVpbxUsersResponse>(
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

			var response = await _apiClient.PostAsync<CreateVpbxMemberRequest, CreateVpbxMemberResponse>(
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

			await _apiClient.PostAsync<DeleteVpbxMemberRequest, VpbxCommandResponse>(
				_deleteMemberEndpoint,
				request,
				true,
				cancellationToken);

			_logger.LogInformation("Сотрудник ВАТС Манго {UserId} удалён", userId);
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<VpbxGroup>> GetAllGroupsAsync(CancellationToken cancellationToken)
		{
			return await GetGroupsAsync(null, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<VpbxGroup>> GetGroupsAsync(long groupId, CancellationToken cancellationToken)
		{
			return await GetGroupsAsync(groupId.ToString(), cancellationToken);
		}

		/// <inheritdoc/>
		private async Task<IReadOnlyList<VpbxGroup>> GetGroupsAsync(string groupId, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Запрашиваем группы ВАТС Манго{GroupIdPart}",
				string.IsNullOrWhiteSpace(groupId) ? string.Empty : $" с идентификатором {groupId}");

			var request = new GetVpbxGroupsRequest
			{
				GroupId = string.IsNullOrWhiteSpace(groupId) ? null : groupId
			};

			var response = await _apiClient.PostAsync<GetVpbxGroupsRequest, GetVpbxGroupsResponse>(
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
			long groupId,
			IEnumerable<long> operatorIds,
			CancellationToken cancellationToken)
		{
			if(operatorIds is null)
			{
				throw new ArgumentNullException(nameof(operatorIds));
			}

			var operators = operatorIds
				.Select(x => new VpbxGroupOperatorUpdate { Id = x.ToString() })
				.ToList();

			_logger.LogInformation(
				"Устанавливаем состав группы ВАТС Манго {GroupId}: {OperatorsCount} сотрудников",
				groupId,
				operators.Count);

			var request = new UpdateVpbxGroupRequest
			{
				GroupId = groupId.ToString(),
				Group = new VpbxGroupUpdate
				{
					Operators = operators
				}
			};

			await _apiClient.PostAsync<UpdateVpbxGroupRequest, VpbxCommandResponse>(
				_updateGroupEndpoint,
				request,
				true,
				cancellationToken);

			_logger.LogInformation("Состав группы ВАТС Манго {GroupId} обновлён", groupId);
		}
	}
}
