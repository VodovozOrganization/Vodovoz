using Mango.Core.Dto.Vpbx.Requests;
using Mango.Core.Dto.Vpbx.Responses;
using Mango.Employees.Library.Options;
using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Mango;

namespace Mango.Employees.Library.Services
{
	/// <summary>
	/// Обрабатывает заявки на регистрацию водителей как сотрудников Манго: подбирает добавочный номер
	/// и создаёт сотрудника. Созданный сотрудник добавляется в группу водителей Манго
	/// </summary>
	public class DriverMangoEmployeeRegistrationService
	{
		private const string _noFreeExtensionNumberError = "Отсутствуют свободные добавочные номера Манго в пуле";
		private const int _reconcileDriversGroupMaxAttempts = 3;
		private const int _delayBetweenReconcileDriversGroupAttemptsInSeconds = 2;

		private readonly ILogger<DriverMangoEmployeeRegistrationService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDriverMangoEmployeeRegistrationRequestRepository _requestRepository;
		private readonly IDriverMangoExtensionNumberRepository _extensionNumberRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IMangoVpbxEmployeesService _mangoVpbxEmployeesService;
		private readonly IOptions<DriverMangoEmployeeRegistrationOptions> _options;

		public DriverMangoEmployeeRegistrationService(
			ILogger<DriverMangoEmployeeRegistrationService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDriverMangoEmployeeRegistrationRequestRepository requestRepository,
			IDriverMangoExtensionNumberRepository extensionNumberRepository,
			IEmployeeRepository employeeRepository,
			IMangoVpbxEmployeesService mangoVpbxEmployeesService,
			IOptions<DriverMangoEmployeeRegistrationOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
			_extensionNumberRepository = extensionNumberRepository ?? throw new ArgumentNullException(nameof(extensionNumberRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_mangoVpbxEmployeesService = mangoVpbxEmployeesService ?? throw new ArgumentNullException(nameof(mangoVpbxEmployeesService));
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}

		/// <summary>
		/// Обрабатывает все новые заявки, каждую в отдельной единице работы
		/// </summary>
		public async Task ProcessNewRequestsAsync(CancellationToken cancellationToken)
		{
			IReadOnlyList<int> requestIds;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Выборка новых заявок на регистрацию сотрудников Манго"))
			{
				requestIds = await _requestRepository.GetNewRequestIdsAsync(uow, cancellationToken);
			}

			if(requestIds.Count == 0)
			{
				return;
			}

			_logger.LogInformation("Найдено {RequestsCount} новых заявок на регистрацию сотрудников Манго", requestIds.Count);

			var mangoUsers = await _mangoVpbxEmployeesService.GetAllUsersAsync(cancellationToken);

			var occupiedMangoExtensions = GetOccupiedExtensions(mangoUsers);

			var createdMangoUserIds = new HashSet<long>();

			foreach(var requestId in requestIds)
			{
				cancellationToken.ThrowIfCancellationRequested();

				await ProcessRequestAsync(requestId, occupiedMangoExtensions, createdMangoUserIds, cancellationToken);
			}

			await ReconcileDriversGroupAsync(mangoUsers, createdMangoUserIds, cancellationToken);
		}

		private async Task ProcessRequestAsync(
			int requestId,
			HashSet<int> occupiedMangoExtensions,
			HashSet<long> createdMangoUserIds,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Обработка заявки на регистрацию сотрудника Манго {requestId}");

			var request = await _requestRepository.GetByIdAsync(uow, requestId, cancellationToken);

			if(request is null || request.Status != DriverMangoEmployeeRegistrationRequestStatus.New)
			{
				return;
			}

			try
			{
				var driver = await _employeeRepository.GetEmployeeByIdAsync(uow, request.DriverId, cancellationToken);

				var validationError = ValidateDriver(request.DriverId, driver);
				if(validationError != null)
				{
					await CompleteWithError(uow, request, validationError, cancellationToken);
					return;
				}

				var driverHasActiveExtensionNumber =
					await _extensionNumberRepository.HasActiveExtensionNumberAsync(uow, driver.Id, cancellationToken);

				if(driverHasActiveExtensionNumber)
				{
					_logger.LogInformation(
						"У водителя {DriverId} уже есть активный добавочный номер, заявка {RequestId} помечена как повторная",
						driver.Id,
						requestId);

					await Complete(uow, request, DriverMangoEmployeeRegistrationRequestStatus.Duplicate, cancellationToken);
					return;
				}

				var extension = await FindFreeExtensionNumberAsync(uow, occupiedMangoExtensions, cancellationToken);
				if(extension is null)
				{
					_logger.LogError(
						"{Error} ({PoolStart}-{PoolEnd}). Заявка {RequestId} не может быть обработана",
						_noFreeExtensionNumberError,
						_options.Value.ExtensionNumberPoolStart,
						_options.Value.ExtensionNumberPoolEnd,
						requestId);

					await CompleteWithError(uow, request, _noFreeExtensionNumberError, cancellationToken);
					return;
				}

				var mangoUserId = await CreateMangoMemberAsync(driver, extension.Value, cancellationToken);

				await SaveExtensionNumberAsync(uow, driver.Id, extension.Value, mangoUserId, request, cancellationToken);
				await Complete(uow, request, DriverMangoEmployeeRegistrationRequestStatus.Completed, cancellationToken);

				occupiedMangoExtensions.Add(extension.Value);
				createdMangoUserIds.Add(mangoUserId);

				_logger.LogInformation(
					"Заявка {RequestId} обработана: водитель {DriverId} зарегистрирован в Манго с номером {Extension}",
					requestId,
					driver.Id,
					extension.Value);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка обработки заявки {RequestId} на регистрацию сотрудника Манго", requestId);

				await CompleteWithError(uow, request, e.Message, cancellationToken);
			}
		}

		private string ValidateDriver(int driverId, Employee driver)
		{
			if(driver is null)
			{
				return $"Водитель с идентификатором {driverId} не найден";
			}

			if(!driver.NeedToCreateMangoEmployee)
			{
				return $"Для сотрудника {driverId} не требуется создание сотрудника Манго";
			}

			if(driver.Status == EmployeeStatus.IsFired)
			{
				return $"Водитель {driverId} уволен";
			}

			var digitsNumber = driver.PhoneForCounterpartyCalls.DigitsNumber;
			if(string.IsNullOrWhiteSpace(digitsNumber) || digitsNumber.Length != 10)
			{
				return $"У водителя {driverId} указан некорректный мобильный телефон для звонков контрагентам";
			}

			return null;
		}

		/// <summary>
		/// Подбирает минимальный свободный добавочный номер из пула: не занятый ни в базе, ни в ВАТС
		/// </summary>
		private async Task<int?> FindFreeExtensionNumberAsync(
			IUnitOfWork uow,
			HashSet<int> occupiedMangoExtensions,
			CancellationToken cancellationToken)
		{
			var usedNumbers =
				(await _extensionNumberRepository.GetUsedExtensionNumbersAsync(uow, cancellationToken)).ToHashSet();
			usedNumbers.UnionWith(occupiedMangoExtensions);

			var poolStart = _options.Value.ExtensionNumberPoolStart;
			var poolEnd = _options.Value.ExtensionNumberPoolEnd;

			for(var extension = poolStart; extension <= poolEnd; extension++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if(!usedNumbers.Contains(extension))
				{
					return extension;
				}
			}

			return null;
		}

		/// <summary>
		/// Возвращает добавочные номера, уже занятые сотрудниками ВАТС, из полученного списка сотрудников
		/// </summary>
		/// <param name="mangoUsers">Сотрудники ВАТС</param>
		/// <returns></returns>
		private static HashSet<int> GetOccupiedExtensions(IReadOnlyList<VpbxUser> mangoUsers)
		{
			var occupiedExtensions = new HashSet<int>();

			foreach(var mangoUser in mangoUsers)
			{
				if(int.TryParse(mangoUser.Telephony?.Extension, out var extension))
				{
					occupiedExtensions.Add(extension);
				}
			}

			return occupiedExtensions;
		}

		private async Task<long> CreateMangoMemberAsync(Employee driver, int extension, CancellationToken cancellationToken)
		{
			// В DigitsNumber хранятся 10 цифр без ведущей 7, которую ожидает Манго
			var mobileNumber = "7" + driver.PhoneForCounterpartyCalls.DigitsNumber;

			var createRequest = new CreateVpbxMemberRequest
			{
				Name = driver.FullName,
				Extension = extension.ToString(),
				AccessRoleId = _options.Value.AccessRoleId,
				LineId = _options.Value.LineId,
				Numbers = new[]
				{
					new VpbxMemberNumber
					{
						Number = mobileNumber
					}
				}
			};

			return await _mangoVpbxEmployeesService.CreateMemberAsync(createRequest, cancellationToken);
		}

		private async Task SaveExtensionNumberAsync(
			IUnitOfWork uow,
			int driverId,
			int extension,
			long mangoUserId,
			DriverMangoEmployeeRegistrationRequest request,
			CancellationToken cancellationToken)
		{
			var extensionNumber = new DriverMangoExtensionNumber
			{
				DriverId = driverId,
				ExtensionNumber = extension,
				MangoUserId = mangoUserId,
				Status = DriverMangoExtensionNumberStatus.Active,
				ActivatedAt = DateTime.Now,
				Request = request
			};

			await uow.SaveAsync(extensionNumber, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);
		}

		private async Task ReconcileDriversGroupAsync(
			IReadOnlyList<VpbxUser> mangoUsers,
			HashSet<long> createdMangoUserIds,
			CancellationToken cancellationToken)
		{
			var driverUserIds = GetPoolUserIds(mangoUsers);
			driverUserIds.UnionWith(createdMangoUserIds);

			if(driverUserIds.Count == 0)
			{
				return;
			}

			for(var attempt = 1; attempt <= _reconcileDriversGroupMaxAttempts; attempt++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				try
				{
					await UpdateDriversGroupAsync(driverUserIds, cancellationToken);

					return;
				}
				catch(Exception e)
				{
					_logger.LogError(
						e,
						"Ошибка обновления состава группы водителей Манго (попытка {Attempt} из {MaxAttempts})",
						attempt,
						_reconcileDriversGroupMaxAttempts);

					if(attempt == _reconcileDriversGroupMaxAttempts)
					{
						return;
					}

					await Task.Delay(
						TimeSpan.FromSeconds(_delayBetweenReconcileDriversGroupAttemptsInSeconds),
						cancellationToken);
				}
			}
		}

		/// <summary>
		/// Возвращает user_id сотрудников ВАТС, у которых добавочный номер входит в пул водителей
		/// </summary>
		/// <param name="mangoUsers">Сотрудники ВАТС</param>
		/// <returns></returns>
		private HashSet<long> GetPoolUserIds(IReadOnlyList<VpbxUser> mangoUsers)
		{
			var poolStart = _options.Value.ExtensionNumberPoolStart;
			var poolEnd = _options.Value.ExtensionNumberPoolEnd;

			var poolUserIds = new HashSet<long>();

			foreach(var mangoUser in mangoUsers)
			{
				if(mangoUser.General?.UserId is null)
				{
					continue;
				}

				if(int.TryParse(mangoUser.Telephony?.Extension, out var extension)
					&& extension >= poolStart
					&& extension <= poolEnd)
				{
					poolUserIds.Add(mangoUser.General.UserId.Value);
				}
			}

			return poolUserIds;
		}

		private async Task UpdateDriversGroupAsync(HashSet<long> poolUserIds, CancellationToken cancellationToken)
		{
			var groupId = _options.Value.DriversGroupId;

			var groups = await _mangoVpbxEmployeesService.GetGroupsAsync(groupId, cancellationToken);

			var group = groups.FirstOrDefault();

			// UpdateGroup заменяет состав целиком, поэтому сохраняем текущих операторов
			var operatorIds = new HashSet<long>(
				group?.Operators?.Select(x => x.Id) ?? Enumerable.Empty<long>());

			var operatorsCountBeforeUnion = operatorIds.Count;

			operatorIds.UnionWith(poolUserIds.Select(id => id));

			if(operatorIds.Count == operatorsCountBeforeUnion)
			{
				return;
			}

			await _mangoVpbxEmployeesService.UpdateGroupOperatorsAsync(groupId, operatorIds, cancellationToken);
		}

		private async Task Complete(
			IUnitOfWork uow,
			DriverMangoEmployeeRegistrationRequest request,
			DriverMangoEmployeeRegistrationRequestStatus status,
			CancellationToken cancellationToken)
		{
			request.Status = status;
			request.ProcessedAt = DateTime.Now;

			await uow.SaveAsync(request, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);
		}

		private async Task CompleteWithError(
			IUnitOfWork uow,
			DriverMangoEmployeeRegistrationRequest request,
			string errorMessage,
			CancellationToken cancellationToken)
		{
			request.Status = DriverMangoEmployeeRegistrationRequestStatus.Error;
			request.ErrorMessage = errorMessage;
			request.ProcessedAt = DateTime.Now;

			await uow.SaveAsync(request, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);
		}
	}
}
