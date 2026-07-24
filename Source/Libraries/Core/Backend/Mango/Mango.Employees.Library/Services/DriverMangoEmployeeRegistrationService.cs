using Mango.Core.Dto.Vpbx.Requests;
using Mango.Employees.Library.Options;
using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
	/// Обрабатывает заявки на регистрацию водителей как сотрудников Манго:
	/// подбирает добавочный номер, создаёт сотрудника и добавляет его в группу
	/// </summary>
	public class DriverMangoEmployeeRegistrationService
	{
		/// <summary>
		/// Сообщение об отсутствии свободных добавочных номеров в пуле
		/// </summary>
		private const string _noFreeExtensionNumberError = "Отсутствуют свободные добавочные номера Манго в пуле";

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

			foreach(var requestId in requestIds)
			{
				cancellationToken.ThrowIfCancellationRequested();

				await ProcessRequestAsync(requestId, cancellationToken);
			}
		}

		private async Task ProcessRequestAsync(int requestId, CancellationToken cancellationToken)
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

				var extension = await FindFreeExtensionNumberAsync(uow, cancellationToken);
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

				// Сохраняем добавочный номер сразу после создания сотрудника в Манго:
				// если добавление в группу далее упадёт, повторной попытки создания (и ошибки занятого номера) не будет
				await SaveExtensionNumberAsync(uow, driver.Id, extension.Value, mangoUserId, cancellationToken);

				await AddMemberToGroupAsync(mangoUserId, cancellationToken);

				await Complete(uow, request, DriverMangoEmployeeRegistrationRequestStatus.Completed, cancellationToken);

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

		private async Task<int?> FindFreeExtensionNumberAsync(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var usedNumbers = new HashSet<int>(await _extensionNumberRepository.GetUsedExtensionNumbersAsync(uow, cancellationToken));

			var poolStart = _options.Value.ExtensionNumberPoolStart;
			var poolEnd = _options.Value.ExtensionNumberPoolEnd;

			for(var extension = poolStart; extension <= poolEnd; extension++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if(usedNumbers.Contains(extension))
				{
					continue;
				}

				var mangoUsers = await _mangoVpbxEmployeesService.GetUsersAsync(extension.ToString(), cancellationToken);
				if(mangoUsers.Count == 0)
				{
					return extension;
				}
			}

			return null;
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

		private async Task SaveExtensionNumberAsync(IUnitOfWork uow, int driverId, int extension, long mangoUserId, CancellationToken cancellationToken)
		{
			var extensionNumber = new DriverMangoExtensionNumber
			{
				DriverId = driverId,
				ExtensionNumber = extension,
				MangoUserId = mangoUserId,
				Status = DriverMangoExtensionNumberStatus.Active,
				ActivatedAt = DateTime.Now
			};

			await uow.SaveAsync(extensionNumber, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);
		}

		private async Task AddMemberToGroupAsync(long mangoUserId, CancellationToken cancellationToken)
		{
			var groupId = _options.Value.DriversGroupId;

			var groups = await _mangoVpbxEmployeesService.GetGroupsAsync(groupId, cancellationToken);

			var group = groups.FirstOrDefault();

			var operatorIds = group?.Operators?
				.Select(x => x.Id.ToString())
				.ToList()
				?? new List<string>();

			var newMemberId = mangoUserId.ToString();
			if(!operatorIds.Contains(newMemberId))
			{
				operatorIds.Add(newMemberId);
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
