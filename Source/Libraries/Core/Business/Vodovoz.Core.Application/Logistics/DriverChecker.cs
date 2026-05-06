using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Common;
using VodovozBusiness.Services.Logistics;

namespace Vodovoz.Core.Application.Logistics
{
	/// <inheritdoc/>
	// Класс содержит IInteractiveService, если его будут использовать в сервисах, то надо там отдельно его реализовывать
	public class DriverChecker : IDriverChecker
	{
		private readonly IGeneralSettings _generalSettings;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IInteractiveService _interactiveService;

		public DriverChecker(
			IGeneralSettings generalSettings,
			IRouteListRepository routeListRepository,
			ICurrentPermissionService currentPermissionService,
			IInteractiveService interactiveService
			)
		{
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		/// <inheritdoc/>
		public bool IsDriversDebtInPermittedRangeVerification(
			IUnitOfWork uow,
			Employee driver,
			int routeListId)
		{
			var result = IsDriverInStopList(uow, driver, routeListId, out var message);

			if(!result)
			{
				return true;
			}

			var canEditDriversStopListParameters =
				_currentPermissionService.ValidatePresetPermission("can_edit_drivers_stop_list_parameters");

			if(canEditDriversStopListParameters)
			{
				message += "\n\nВсе равно продолжить?";
				return _interactiveService.Question(message, "Требуется подтверждение");
			}

			_interactiveService.ShowMessage(ImportanceLevel.Error, message);
			return false;
		}
		
		private bool IsDriverInStopList(
			IUnitOfWork uow,
			Employee driver,
			int routeListId,
			out string message)
		{
			message = null;
			
			if(driver is null)
			{
				return false;
			}
			
			var maxDriversUnclosedRouteListsCountParameter = _generalSettings.DriversUnclosedRouteListsHavingDebtMaxCount;
			var maxDriversRouteListsDebtsSumParameter = _generalSettings.DriversRouteListsMaxDebtSum;

			var isDriverHasActiveStopListRemoval = driver.IsDriverHasActiveStopListRemoval(uow);

			if(isDriverHasActiveStopListRemoval)
			{
				return false;
			}

			var unclosedRouteListsHavingDebtsCount =
				_routeListRepository.GetUnclosedRouteListsCountHavingDebtByDriver(uow, driver.Id, routeListId);
			var unclosedRouteListsDebtsSum =
				_routeListRepository.GetUnclosedRouteListsDebtsSumByDriver(uow, driver.Id, routeListId);

			if(unclosedRouteListsHavingDebtsCount > maxDriversUnclosedRouteListsCountParameter 
				|| unclosedRouteListsDebtsSum > maxDriversRouteListsDebtsSumParameter)
			{
				message =
					$"Водитель {driver.FullName} в стоп-листе, т.к. кол-во незакрытых МЛ с долгом {unclosedRouteListsHavingDebtsCount} штук " +
					$"и суммарный долг водителя по всем МЛ составляет {unclosedRouteListsDebtsSum} рублей.";
				
				return true;
			}
			
			return false;
		}
	}
}
