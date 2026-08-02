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
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Mango;

namespace Mango.Employees.Library.Services
{
	/// <summary>
	/// Деактивирует активные добавочные номера водителей: удаляет сотрудника из Манго,
	/// если у водителя нет маршрутных листов в работе
	/// </summary>
	public class DriverMangoEmployeeDeactivationService
	{
		private static readonly RouteListStatus[] _blockingRouteListStatuses =
		{
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute
		};

		private readonly ILogger<DriverMangoEmployeeDeactivationService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDriverMangoExtensionNumberRepository _extensionNumberRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IMangoVpbxEmployeesService _mangoVpbxEmployeesService;
		private readonly IOptions<DriverMangoEmployeeDeactivationOptions> _options;

		public DriverMangoEmployeeDeactivationService(
			ILogger<DriverMangoEmployeeDeactivationService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDriverMangoExtensionNumberRepository extensionNumberRepository,
			IRouteListRepository routeListRepository,
			IMangoVpbxEmployeesService mangoVpbxEmployeesService,
			IOptions<DriverMangoEmployeeDeactivationOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_extensionNumberRepository = extensionNumberRepository ?? throw new ArgumentNullException(nameof(extensionNumberRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_mangoVpbxEmployeesService = mangoVpbxEmployeesService ?? throw new ArgumentNullException(nameof(mangoVpbxEmployeesService));
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}

		/// <summary>
		/// Обрабатывает активные добавочные номера, активированные раньше указанной даты,
		/// каждый в отдельной единице работы
		/// </summary>
		/// <param name="activatedBefore">
		/// Верхняя граница даты активации (не включительно). Обычно - начало текущих суток по МСК,
		/// чтобы не деактивировать номера, выделенные сегодня
		/// </param>
		public async Task ProcessActiveExtensionNumbersAsync(DateTime activatedBefore, CancellationToken cancellationToken)
		{
			IReadOnlyList<int> extensionNumberIds;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Выборка активных добавочных номеров Манго"))
			{
				var activeExtensionNumbers =
					await _extensionNumberRepository.GetActiveExtensionNumbersAsync(uow, activatedBefore, cancellationToken);

				extensionNumberIds = activeExtensionNumbers
					.Select(x => x.Id)
					.ToList();
			}

			if(extensionNumberIds.Count == 0)
			{
				return;
			}

			_logger.LogInformation("Найдено {ExtensionNumbersCount} активных добавочных номеров Манго", extensionNumberIds.Count);

			foreach(var extensionNumberId in extensionNumberIds)
			{
				cancellationToken.ThrowIfCancellationRequested();

				await ProcessExtensionNumberAsync(extensionNumberId, cancellationToken);
			}
		}

		private async Task ProcessExtensionNumberAsync(int extensionNumberId, CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Деактивация добавочного номера Манго {extensionNumberId}");

			var extensionNumber = await _extensionNumberRepository.GetByIdAsync(uow, extensionNumberId, cancellationToken);

			if(extensionNumber is null || extensionNumber.Status != DriverMangoExtensionNumberStatus.Active)
			{
				return;
			}

			try
			{
				var driverHasRouteListsInWork = await _routeListRepository.HasDriverRouteListsInStatusesAsync(
					uow, extensionNumber.DriverId, _blockingRouteListStatuses, cancellationToken);

				if(driverHasRouteListsInWork)
				{
					_logger.LogInformation(
						"У водителя {DriverId} есть маршрутные листы в работе, добавочный номер {ExtensionNumber} не деактивируется",
						extensionNumber.DriverId,
						extensionNumber.ExtensionNumber);

					return;
				}

				if(!extensionNumber.MangoUserId.HasValue)
				{
					_logger.LogWarning(
						"У добавочного номера {ExtensionNumberId} не указан идентификатор сотрудника Манго, деактивация пропущена",
						extensionNumberId);

					return;
				}

				if(!IsExtensionNumberInPool(extensionNumber.ExtensionNumber))
				{
					_logger.LogWarning(
						"Добавочный номер {ExtensionNumber} не входит в пул {PoolStart}-{PoolEnd}, "
							+ "удаление сотрудника Манго через API не выполняется",
						extensionNumber.ExtensionNumber,
						_options.Value.ExtensionNumberPoolStart,
						_options.Value.ExtensionNumberPoolEnd);

					return;
				}

				await _mangoVpbxEmployeesService.DeleteMemberAsync(extensionNumber.MangoUserId.Value.ToString(), cancellationToken);

				extensionNumber.Status = DriverMangoExtensionNumberStatus.Deactivated;
				extensionNumber.DeactivatedAt = DateTime.Now;

				await uow.SaveAsync(extensionNumber, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Добавочный номер {ExtensionNumber} водителя {DriverId} деактивирован",
					extensionNumber.ExtensionNumber,
					extensionNumber.DriverId);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка деактивации добавочного номера {ExtensionNumberId} Манго",
					extensionNumberId);
			}
		}

		private bool IsExtensionNumberInPool(int? extensionNumber) =>
			extensionNumber.HasValue
			&& extensionNumber.Value >= _options.Value.ExtensionNumberPoolStart
			&& extensionNumber.Value <= _options.Value.ExtensionNumberPoolEnd;
	}
}
