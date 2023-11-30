using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using RouteList = Vodovoz.Domain.Logistic.RouteList;

namespace Vodovoz.Application.Services.Logistics
{
	public class RouteListService : IRouteListService
	{
		private readonly ILogger<RouteListService> _logger;
		private readonly ITerminalNomenclatureProvider _terminalNomenclatureProvider;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IGenericRepository<RouteListSpecialCondition> _routeListSpecialConditionRepository;
		private readonly IGenericRepository<RouteListSpecialConditionType> _routeListSpecialConditionTypeRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly ICallTaskWorker _callTaskWorker;

		private readonly IRouteOptimizer _routeOptimizer;
		private readonly IRouteListAddressKeepingDocumentController _routeListKeepingDocumentController;
		private readonly IWageParameterService _wageParameterService;
		IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly ITrackRepository _trackRepository;
		private readonly RouteGeometryCalculator _routeGeometryCalculator;

		public RouteListService(
			ILogger<RouteListService> logger,
			ITerminalNomenclatureProvider terminalNomenclatureProvider,
			IRouteListRepository routeListRepository,
			IGenericRepository<RouteListSpecialCondition> routeListSpecialConditionRepository,
			IGenericRepository<RouteListSpecialConditionType> routeListSpecialConditionTypeRepository,
			IGenericRepository<Employee> employeeRepository,
			ICallTaskWorker callTaskWorker,
			IRouteOptimizer routeOptimizer,
			IRouteListAddressKeepingDocumentController routeListKeepingDocumentController,
			IWageParameterService wageParameterService,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IRouteListProfitabilityController routeListProfitabilityController,
			ITrackRepository trackRepository,
			RouteGeometryCalculator routeGeometryCalculator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_terminalNomenclatureProvider = terminalNomenclatureProvider
				?? throw new ArgumentNullException(nameof(terminalNomenclatureProvider));
			_routeListRepository = routeListRepository
				?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListSpecialConditionRepository = routeListSpecialConditionRepository
				?? throw new ArgumentNullException(nameof(routeListSpecialConditionRepository));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_routeOptimizer = routeOptimizer ?? throw new ArgumentNullException(nameof(routeOptimizer));
			_routeListKeepingDocumentController = routeListKeepingDocumentController ?? throw new ArgumentNullException(nameof(routeListKeepingDocumentController));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeGeometryCalculator = routeGeometryCalculator ?? throw new ArgumentNullException(nameof(routeGeometryCalculator));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_routeListSpecialConditionTypeRepository = routeListSpecialConditionTypeRepository
				?? throw new ArgumentNullException(nameof(routeListSpecialConditionTypeRepository));
		}

		public IDictionary<int, string> GetSpecialConditionsDictionaryFor(
			IUnitOfWork unitOfWork,
			int routeListId)
		{
			var routeListConditions = _routeListSpecialConditionRepository
				.Get(unitOfWork, x => x.RouteListId == routeListId);

			var conditionTypesIds = routeListConditions.Select(x => x.RouteListSpecialConditionTypeId).Distinct();

			var specialConditrionTypes = _routeListSpecialConditionTypeRepository.Get(unitOfWork, sct => conditionTypesIds.Contains(sct.Id));

			var result = routeListConditions.ToDictionary(x => x.Id, x => specialConditrionTypes.First(sct => sct.Id == x.RouteListSpecialConditionTypeId).Name);

			return result;
		}

		public IEnumerable<RouteListSpecialCondition> GetSpecialConditionsFor(
			IUnitOfWork unitOfWork,
			int routeListId)
		{
			var routeList = _routeListSpecialConditionRepository
				.Get(unitOfWork, x => x.RouteListId == routeListId);

			return routeList;
		}

		public bool TrySendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			out IList<GoodsInRouteListResult> notLoadedGoods,
			CarLoadDocument withDocument = null)
		{
			notLoadedGoods = new List<GoodsInRouteListResult>();
			var terminalId = _terminalNomenclatureProvider.GetNomenclatureIdForTerminal;

			var terminalsTransferedToThisRL = _routeListRepository.TerminalTransferedCountToRouteList(unitOfWork, routeList);

			var itemsInLoadDocuments = _routeListRepository.AllGoodsLoaded(unitOfWork, routeList);

			if(withDocument != null)
			{
				foreach(var item in withDocument.Items)
				{
					var found = itemsInLoadDocuments.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id);
					if(found != null)
					{
						found.Amount += item.Amount;
					}
					else
					{
						itemsInLoadDocuments.Add(new GoodsInRouteListResult { NomenclatureId = item.Nomenclature.Id, Amount = item.Amount });
					}
				}
			}

			var allItemsToLoad = _routeListRepository.GetGoodsAndEquipsInRL(unitOfWork, routeList);

			bool closed = true;
			foreach(var itemToLoad in allItemsToLoad)
			{
				var loaded = itemsInLoadDocuments.FirstOrDefault(x => x.NomenclatureId == itemToLoad.NomenclatureId);

				if(itemToLoad.NomenclatureId == terminalId
					&& ((loaded?.Amount ?? 0) + terminalsTransferedToThisRL == itemToLoad.Amount
						|| _routeListRepository.GetSelfDriverTerminalTransferDocument(unitOfWork, routeList.Driver, routeList) != null))
				{
					continue;
				}

				var notLoadedAmount = itemToLoad.Amount - (loaded?.Amount ?? 0);
				if(notLoadedAmount == 0)
				{
					continue;
				}

				notLoadedGoods.Add(new GoodsInRouteListResult { NomenclatureId = itemToLoad.NomenclatureId, Amount = notLoadedAmount });
				closed = false;
			}

			if(closed)
			{
				if(routeList.NotFullyLoaded.HasValue)
				{
					routeList.NotFullyLoaded = false;
				}

				if(RouteList.AvailableToSendEnRouteStatuses.Contains(routeList.Status))
				{
					SendEnRoute(unitOfWork, routeList);
				}
			}

			return closed;
		}

		public void SendEnRoute(
			IUnitOfWork unitOfWork,
			int routeListId)
		{
			using(var transaction = unitOfWork.Session.BeginTransaction())
			{
				var routeList = _routeListRepository.GetRouteListById(unitOfWork, routeListId);

				if(routeList is null)
				{
					_logger.LogWarning("Маршрутный лист с номером {RouteListId} не найден, не удалось отправить в путь", routeListId);

					return;
				}

				SendEnRoute(unitOfWork, routeList);

				transaction.Commit();
			}
		}

		public void SendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList)
		{
			if(routeList is null)
			{
				_logger.LogWarning("Маршрутный лист с номером {RouteListId} не найден, не удалось отправить в путь", routeList.Id);

				return;
			}

			if(!routeList.SpecialConditionsAccepted)
			{
				_logger.LogTrace("Специальыне условия не приняты. Добавление отсутствующих");

				CreateSpecialConditionsFor(unitOfWork, routeList);
			}

			routeList.ChangeStatusAndCreateTask(RouteListStatus.EnRoute, _callTaskWorker);
		}

		private void CreateSpecialConditionsFor(
			IUnitOfWork unitOfWork,
			RouteList routeList)
		{
			var existingSpecialConditions = GetSpecialConditionsFor(unitOfWork, routeList.Id);

			if(routeList.Addresses.Any(x => x.Order.HasCommentForDriver)
				&& !existingSpecialConditions.Any(x =>
					x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.OrdersHasComments))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.OrdersHasComments
				});
			}

			if(routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal)
				&& !existingSpecialConditions.Any(x =>
					x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.OrdersRequireTerminal))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.OrdersRequireTerminal
				});
			}

			if(routeList.Addresses.Any(x => x.Order.Trifle > 0)
				&& !existingSpecialConditions.Any(x =>
					x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.OrdersRequireTrifle))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.OrdersRequireTrifle
				});
			}

			if(routeList.AdditionalLoadingDocument != null
				&& !existingSpecialConditions.Any(x =>
					x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.RouteListRequireAdditionalLoading))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.RouteListRequireAdditionalLoading
				});
			}
		}

		public void AcceptConditions(
			IUnitOfWork unitOfWork,
			int driverId,
			IEnumerable<int> specialConditionsIds)
		{
			var specialConditions = _routeListSpecialConditionRepository.Get(unitOfWork, sc => specialConditionsIds.Contains(sc.Id));

			var firstRouteListId = specialConditions.FirstOrDefault()?.RouteListId;

			if(!specialConditions.All(x => x.RouteListId == firstRouteListId))
			{
				var tooManyRouteListsErrorTemplate = "Нельзя принять сразу условия несколькоих Маршрутных Листов";

				_logger.LogError(
					"Попытка принять условия нескольких МЛ водителем {DriverId}. Идентификаторы специальных условий: {$SpecialConditionsIds}",
					driverId,
					specialConditions.Select(x => x.Id));

				throw new ArgumentException(tooManyRouteListsErrorTemplate, nameof(specialConditions));
			}

			var driver = _employeeRepository.Get(unitOfWork, x => x.Id == driverId).FirstOrDefault();

			if(driver is null)
			{
				var driverNotFoundErrorTemplate = "Не накйден водитель с идентификатором {DriverId}";

				_logger.LogError(driverNotFoundErrorTemplate, driverId);

				throw new ArgumentException(string.Format(driverNotFoundErrorTemplate, driverId), nameof(driverId));
			}

			var routeList = _routeListRepository.GetDriverRouteLists(unitOfWork, driver).Where(x => x.Id == firstRouteListId).FirstOrDefault();

			if(routeList is null)
			{
				var driverRouteListNotFoundErrorTemplate = "Не найден МЛ водителя {DriverId} номер {RouteListId}";

				_logger.LogError(driverRouteListNotFoundErrorTemplate, driverId, firstRouteListId);

				throw new InvalidOperationException(string.Format(driverRouteListNotFoundErrorTemplate, driverId, firstRouteListId));
			}

			var routeListConditions = GetSpecialConditionsFor(unitOfWork, firstRouteListId.Value);

			if(!routeListConditions.All(x => specialConditions.Any(sc => sc.Id == x.Id)))
			{
				throw new ArgumentException("Нельзя принять не все условия МЛ", nameof(specialConditionsIds));
			}

			using(var transaction = unitOfWork.Session.BeginTransaction())
			{
				foreach(var specialCondition in routeListConditions)
				{
					specialCondition.Accepted = true;
					unitOfWork.Save(specialCondition);
				}

				routeList.SpecialConditionsAccepted = true;
				routeList.SpecialConditionsAcceptedAt = DateTime.Now;
				unitOfWork.Save(routeList);

				transaction.Commit();
			}
		}

		public Result<RouteListAcceptStatus> TryAcceptOrEditRouteList(IUnitOfWork uow, RouteList routeList, bool isAcceptMode, Action<bool> disableItemsUpdate, ICommonServices commonServices)
		{
			if(routeList.Car == null)
			{
				return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.CarIsEmpty);
			}

			StringBuilder warningMsg = new StringBuilder($"Автомобиль '{routeList.Car.Title}':");

			if(routeList.HasOverweight())
			{
				warningMsg.Append($"\n\t- перегружен на {routeList.Overweight()} кг");
			}

			if(routeList.HasVolumeExecess())
			{
				warningMsg.Append($"\n\t- объём груза превышен на {routeList.VolumeExecess()} м<sup>3</sup>");
			}

			if(routeList.HasReverseVolumeExcess())
			{
				warningMsg.Append($"\n\t- объём возвращаемого груза превышен на {routeList.ReverseVolumeExecess()} м<sup>3</sup>");
			}

			if(isAcceptMode && (routeList.HasOverweight() || routeList.HasVolumeExecess() || routeList.HasReverseVolumeExcess()))
			{
				if(commonServices.CurrentPermissionService.ValidatePresetPermission("can_confirm_routelist_with_overweight"))
				{
					warningMsg.AppendLine("\nВы уверены что хотите подтвердить маршрутный лист?");
					if(!commonServices.InteractiveService.Question(warningMsg.ToString()))
					{
						return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.CreateHasOverweight(warningMsg.ToString()));
					}
				}
				else
				{
					warningMsg.AppendLine("\nПодтвердить маршрутный лист нельзя.");
					return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.CreateHasOverweight(warningMsg.ToString()));
				}
			}

			var result = isAcceptMode
				? TryChangeStatusToAccepted(uow, routeList, disableItemsUpdate, commonServices)
				: TryChangeStatusToNew(uow, routeList);

			// -= From Save
			RecalculateRouteList(uow, routeList);
			// From Save =-

			return result;
		}

		private void RecalculateRouteList(IUnitOfWork uow, RouteList routeList)
		{
			routeList.CalculateWages(_wageParameterService);

			var commonFastDeliveryMaxDistance = (decimal)_deliveryRulesParametersProvider.GetMaxDistanceToLatestTrackPointKmFor(DateTime.Now);
			routeList.UpdateFastDeliveryMaxDistanceValue(commonFastDeliveryMaxDistance);

			_logger.LogDebug("Пересчитываем рентабельность МЛ");

			_routeListProfitabilityController.ReCalculateRouteListProfitability(uow, routeList);
			_logger.LogDebug("Закончили пересчет рентабельности МЛ");
			uow.Save(routeList.RouteListProfitability);
		}

		private Result<RouteListAcceptStatus> TryChangeStatusToAccepted(IUnitOfWork uow, RouteList routeList, Action<bool> disableItemsUpdate, ICommonServices commonServices)
		{
			if(routeList.Status != RouteListStatus.New)
			{
				return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.IncorrectStatusForAccept);
			}

			var contextItems = new Dictionary<object, object>
				{
					{ "NewStatus", RouteListStatus.Confirmed },
					{ nameof(IRouteListItemRepository), new RouteListItemRepository() }
				};

			var context = new ValidationContext(routeList, null, contextItems);

			if(!commonServices.ValidationService.Validate(routeList, context))
			{
				return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.ValidationFailure);
			}

			routeList.ChangeStatusAndCreateTask(RouteListStatus.Confirmed, _callTaskWorker);
			//Строим маршрут для МЛ.
			if((!routeList.PrintsHistory?.Any() ?? true) || commonServices.InteractiveService.Question("Этот маршрутный лист уже был когда-то напечатан. При новом построении маршрута порядок адресов может быть другой. При продолжении обязательно перепечатайте этот МЛ.\nПерестроить маршрут?", "Перестроить маршрут?"))
			{

				var newRoute = _routeOptimizer.RebuidOneRoute(routeList);
				if(newRoute != null)
				{
					disableItemsUpdate(true);
					newRoute.UpdateAddressOrderInRealRoute(routeList);
					//Рассчитываем расстояние

					routeList.RecalculatePlanedDistance(_routeGeometryCalculator);
					disableItemsUpdate(false);
					var noPlan = routeList.Addresses.Count(x => !x.PlanTimeStart.HasValue);
					if(noPlan > 0)
					{
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Для маршрута незапланировано {noPlan} адресов.");
					}
				}
				else
				{
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Маршрут не был перестроен.");
				}
			}

			_logger.LogInformation("Создаём операции по свободным остаткам МЛ {RouteListId}...", routeList.Id);

			foreach(var address in routeList.Addresses)
			{
				if(address.TransferedTo == null &&
				   (!address.WasTransfered || address.AddressTransferType != AddressTransferType.FromHandToHand))
				{
					_routeListKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(
						uow, address, DeliveryFreeBalanceType.Decrease, isFullRecreation: true, needRouteListUpdate: true);
				}
				else
				{
					_routeListKeepingDocumentController.RemoveRouteListKeepingDocument(uow, address, true);
				}
			}

			_logger.LogInformation("Операции по свободным остаткакам МЛ {RouteListId} созданы.", routeList.Id);
			if(routeList.GetCarVersion.IsCompanyCar && routeList.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck && !routeList.NeedToLoad)
			{
				if(commonServices.InteractiveService.Question(
					$"Маршрутный лист для транспортировки на склад, перевести машрутный лист сразу в статус '{RouteListStatus.OnClosing.GetEnumTitle()}'?"))
				{
					routeList.CompleteRouteAndCreateTask(_wageParameterService, _callTaskWorker, _trackRepository);
				}
			}
			else
			{
				//Проверяем нужно ли маршрутный лист грузить на складе, если нет переводим в статус в пути.
				var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

				if(!routeList.NeedToLoad && !needTerminal)
				{
					if(commonServices.InteractiveService.Question($"Для маршрутного листа, нет необходимости грузится на складе. Перевести маршрутный лист сразу в статус '{RouteListStatus.EnRoute.GetEnumTitle()}'?"))
					{
						var contextItemsEnroute = new Dictionary<object, object>
							{
								{ "NewStatus", RouteListStatus.EnRoute },
								{ nameof(IRouteListItemRepository), new RouteListItemRepository() }
							};

						var contextEnroute = new ValidationContext(routeList, null, contextItemsEnroute);

						if(!commonServices.ValidationService.Validate(routeList, contextEnroute))
						{
							return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.ValidationFailure);
						}
						else
						{
							SendEnRoute(uow, routeList.Id);
						}
					}
					else
					{
						routeList.ChangeStatusAndCreateTask(RouteListStatus.New, _callTaskWorker);
					}
				}
			}

			return Result.Success(RouteListAcceptStatus.Accepted);
		}

		private Result<RouteListAcceptStatus> TryChangeStatusToNew(IUnitOfWork uow, RouteList routeList)
		{
			if(routeList.Status != RouteListStatus.InLoading && routeList.Status != RouteListStatus.Confirmed)
			{
				return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.IncorrectStatusForEdit);
			}

			if(_routeListRepository.GetCarLoadDocuments(uow, routeList.Id).Any())
			{
				return Result.Failure<RouteListAcceptStatus>(Errors.Logistics.RouteList.HasCarLoadingDocuments);
			}
			else
			{
				routeList.ChangeStatusAndCreateTask(RouteListStatus.New, _callTaskWorker);

				return Result.Success(RouteListAcceptStatus.New);
			}
		}
	}
}
