﻿using Microsoft.Extensions.Logging;
using MoreLinq;
using NHibernate;
using NHibernate.Util;
using QS.DomainModel.UoW;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Application.Logistics.RouteOptimization;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Application.Logistics
{
	public class RouteListService : IRouteListService
	{
		private readonly ILogger<RouteListService> _logger;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IGenericRepository<RouteListSpecialCondition> _routeListSpecialConditionRepository;
		private readonly IGenericRepository<RouteListSpecialConditionType> _routeListSpecialConditionTypeRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IGenericRepository<ProductGroup> _productGroupRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IRouteOptimizer _routeOptimizer;
		private readonly IWageParameterService _wageParameterService;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IAddressTransferController _addressTransferController;
		private readonly IGenericRepository<RouteListAddressKeepingDocument> _routeListAddressKeepingDocumentsRepository;
		private readonly IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly RouteGeometryCalculator _routeGeometryCalculator;
		private readonly IGenericRepository<AddressTransferDocumentItem> _routeListAddressTransferItemRepository;
		private readonly IGenericRepository<RouteListItem> _routeListAddressesRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public RouteListService(
			ILogger<RouteListService> logger,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IGenericRepository<RouteListSpecialCondition> routeListSpecialConditionRepository,
			IGenericRepository<RouteListSpecialConditionType> routeListSpecialConditionTypeRepository,
			IGenericRepository<Employee> employeeRepository,
			IGenericRepository<ProductGroup> productGroupRepository,
			ICallTaskWorker callTaskWorker,
			IRouteOptimizer routeOptimizer,
			IWageParameterService wageParameterService,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IAddressTransferController addressTransferController,
			IGenericRepository<RouteListAddressKeepingDocument> routeListAddressKeepingDocumentsRepository,
			IRouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryRepository deliveryRepository,
			ITrackRepository trackRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			INomenclatureSettings nomenclatureSettings,
			RouteGeometryCalculator routeGeometryCalculator,
			IGenericRepository<AddressTransferDocumentItem> routeListAddressTransferItemRepository,
			IGenericRepository<RouteListItem> routeListAddressesRepository,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_routeListRepository = routeListRepository
				?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository
				?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListSpecialConditionRepository = routeListSpecialConditionRepository
				?? throw new ArgumentNullException(nameof(routeListSpecialConditionRepository));
			_routeListSpecialConditionTypeRepository = routeListSpecialConditionTypeRepository
				?? throw new ArgumentNullException(nameof(routeListSpecialConditionTypeRepository));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_callTaskWorker = callTaskWorker
				?? throw new ArgumentNullException(nameof(callTaskWorker));
			_routeOptimizer = routeOptimizer
				?? throw new ArgumentNullException(nameof(routeOptimizer));
			_wageParameterService = wageParameterService
				?? throw new ArgumentNullException(nameof(wageParameterService));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings
				?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_addressTransferController = addressTransferController
				?? throw new ArgumentNullException(nameof(addressTransferController));
			_routeListAddressKeepingDocumentsRepository = routeListAddressKeepingDocumentsRepository
				?? throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentsRepository));
			_routeListAddressKeepingDocumentController = routeListAddressKeepingDocumentController
				?? throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentController));
			_deliveryRulesSettings = deliveryRulesSettings
				?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_deliveryRepository = deliveryRepository
				?? throw new ArgumentNullException(nameof(deliveryRepository));
			_trackRepository = trackRepository
				?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListProfitabilityController = routeListProfitabilityController
				?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_nomenclatureSettings = nomenclatureSettings
				?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_routeGeometryCalculator = routeGeometryCalculator
				?? throw new ArgumentNullException(nameof(routeGeometryCalculator));
			_productGroupRepository = productGroupRepository
				?? throw new ArgumentNullException(nameof(productGroupRepository));
			_routeListAddressTransferItemRepository = routeListAddressTransferItemRepository
				?? throw new ArgumentNullException(nameof(routeListAddressTransferItemRepository));
			_routeListAddressesRepository = routeListAddressesRepository
				?? throw new ArgumentNullException(nameof(routeListAddressesRepository));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public bool TrySendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			out IList<GoodsInRouteListResult> notLoadedGoods,
			CarLoadDocument withDocument = null)
		{
			notLoadedGoods = new List<GoodsInRouteListResult>();
			var terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;

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

		#region SpecialConditions - спец. условия МЛ

		public IDictionary<int, string> GetSpecialConditionsDictionaryFor(
			IUnitOfWork unitOfWork,
			int routeListId)
		{
			var routeListConditions = _routeListSpecialConditionRepository
				.Get(unitOfWork, x => x.RouteListId == routeListId);

			var conditionTypesIds = routeListConditions
				.Select(x => x.RouteListSpecialConditionTypeId)
				.Distinct();

			var specialConditrionTypes = _routeListSpecialConditionTypeRepository
				.Get(unitOfWork, sct => conditionTypesIds.Contains(sct.Id));

			var result = routeListConditions
				.ToDictionary(x => x.Id, x => specialConditrionTypes
					.First(sct => sct.Id == x.RouteListSpecialConditionTypeId).Name);

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

			var productGroupsIds = GetAllProductGroupChilds(unitOfWork, _nomenclatureSettings.EquipmentForCheckProductGroupsIds);

			if(routeList.Addresses
				.Any(address =>
					address.Order.OrderItems.Any(oi =>
						productGroupsIds.Contains(oi.Nomenclature.ProductGroup.Id))
				&& !existingSpecialConditions.Any(x =>
					x.RouteListSpecialConditionTypeId == RouteListSpecialConditionType.EquipmentCheckRequired)))
			{
				unitOfWork.Save(new RouteListSpecialCondition
				{
					RouteListId = routeList.Id,
					RouteListSpecialConditionTypeId = RouteListSpecialConditionType.EquipmentCheckRequired
				});
			}
		}

		private IEnumerable<int> GetAllProductGroupChilds(IUnitOfWork unitOfWork, IEnumerable<int> ids)
		{
			var result = new List<int>(ids);

			var buffer = new List<int>(ids);

			while(buffer.Count > 0)
			{
				var current = buffer[0];

				var productGroupsIds = _productGroupRepository
					.Get(unitOfWork, pg => pg.Parent.Id == current)
					.Select(pg => pg.Id)
					.ToList();

				buffer.AddRange(productGroupsIds);

				result = result.Union(productGroupsIds).ToList();

				buffer.Remove(current);
			}

			return result;
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

			var routeList = _routeListRepository.GetDriverRouteLists(unitOfWork, driver.Id)
				.FirstOrDefault(x => x.Id == firstRouteListId);

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

		#endregion SpecialConditions - спец. условия МЛ

		#region Transfers - переносы

		public Result<IEnumerable<string>> TransferOrdersTo(
			IUnitOfWork unitOfWork,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> ordersIdsAndTransferType)
		{
			var messages = new List<string>();

			var errors = new List<Error>();

			var targetRouteList = unitOfWork.GetById<RouteList>(targetRouteListId);

			if(targetRouteList is null)
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.CreateNotFound(targetRouteListId));
			}

			var ordersToTransfer = unitOfWork.Session.Query<Order>()
				.Where(x => ordersIdsAndTransferType.Keys.Contains(x.Id));

			foreach(var order in ordersToTransfer)
			{
				var result = TransferOrderTo(unitOfWork, targetRouteList, order, ordersIdsAndTransferType[order.Id]);

				if(result.IsFailure)
				{
					errors.AddRange(result.Errors);
					continue;
				}

				messages.Add(result.Value);
			}

			if(errors.Any())
			{
				return Result.Failure<IEnumerable<string>>(errors);
			}

			return Result.Success(messages.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		public Result<IEnumerable<string>> TransferAddressesFrom(
			IUnitOfWork unitOfWork,
			int sourceRouteListId,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> addressIdsAndTransferType)
		{
			var messages = new List<string>();

			var errors = new List<Error>();

			var sourceRouteList = unitOfWork.GetById<RouteList>(sourceRouteListId);

			var targetRouteList = unitOfWork.GetById<RouteList>(targetRouteListId);

			if(targetRouteList is null)
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.CreateNotFound(targetRouteListId));
			}

			var addressesToTransfer = sourceRouteList.Addresses
				.Where(x => addressIdsAndTransferType.Keys.Contains(x.Id)
					&& x.Status != RouteListItemStatus.Transfered)
				.ToList();

			if(!addressesToTransfer.Any())
			{
				return Result.Success(Enumerable.Empty<string>());
			}

			foreach(var address in addressesToTransfer)
			{
				var result = TransferAddressFrom(unitOfWork, sourceRouteList, targetRouteList, address, addressIdsAndTransferType[address.Id]);

				if(result.IsSuccess)
				{
					messages.AddRange(result.Value);
					continue;
				}

				errors.AddRange(result.Errors);
			}

			if(errors.Any())
			{
				return Result.Failure<IEnumerable<string>>(errors);
			}

			return Result.Success(messages.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		public Result<IEnumerable<string>> RevertTransferedAddressesFrom(
			IUnitOfWork unitOfWork,
			int sourceRouteListId,
			int? targetRouteListId,
			IEnumerable<int> addressIds)
		{
			var messages = new List<string>();
			var errors = new List<string>();

			var sourceRouteList = unitOfWork.Session
				.Query<RouteList>()
				.Where(x => x.Id == sourceRouteListId)
				.FirstOrDefault();

			if(sourceRouteList is null)
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.CreateNotFound(sourceRouteListId));
			}

			var targetRouteList = unitOfWork.Session
				.Query<RouteList>()
				.Where(x => x.Id == targetRouteListId)
				.FirstOrDefault();

			var addressesWithoutSourceAdded = sourceRouteList.Addresses
				.Where(x =>
					addressIds.Contains(x.Id)
					&& !x.WasTransfered
					&& x.AddressTransferType == AddressTransferType.FromFreeBalance
					&& x.Status != RouteListItemStatus.Transfered)
				.ToList();

			foreach(var address in addressesWithoutSourceAdded)
			{
				address.Order.ChangeStatus(OrderStatus.Accepted);

				sourceRouteList.Addresses.Remove(address);

				var routeListKeepingDocument = _routeListAddressKeepingDocumentsRepository
					.Get(unitOfWork, x => x.RouteListItem.Id == address.Id)
					.SingleOrDefault();

				foreach(var item in routeListKeepingDocument.Items)
				{
					sourceRouteList.DeliveryFreeBalanceOperations.Remove(item.DeliveryFreeBalanceOperation);
				}

				unitOfWork.Delete(routeListKeepingDocument);
				unitOfWork.Session.Evict(address);
			}

			var addressesToRevert = sourceRouteList.Addresses
				.Where(x => addressIds.Contains(x.Id)
					&& x.WasTransfered)
				.ToList();

			var revertErrors = new List<Error>();

			foreach(var address in addressesToRevert)
			{
				var result = RevertTransferedAddressFrom(unitOfWork, sourceRouteList, targetRouteList, address);

				if(result.IsSuccess)
				{
					messages.Add(result.Value);

					continue;
				}

				revertErrors.AddRange(result.Errors);
			}

			if(revertErrors.Any())
			{
				return Result.Failure<IEnumerable<string>>(revertErrors);
			}

			sourceRouteList.CalculateWages(_wageParameterService);

			// Сохранение данных в транзакцию

			FlushSessionWithoutCommit(unitOfWork);

			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, sourceRouteList);

			unitOfWork.Save(sourceRouteList);

			if(targetRouteList != null)
			{
				_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, targetRouteList);
				unitOfWork.Save(targetRouteList);
			}

			return Result.Success(messages.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		private Result<string> TransferOrderTo(
			IUnitOfWork unitOfWork,
			RouteList targetRouteList,
			Order order,
			AddressTransferType? transferType)
		{
			if(transferType != AddressTransferType.FromFreeBalance)
			{
				return Result.Failure<string>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateInvalidOrderTransferType(order.Id));
			}

			var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(unitOfWork, order, targetRouteList);

			if(!hasBalanceForTransfer)
			{
				return Result.Failure<string>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateOrderTransferNotEnoughtFreeBalance(order.Id, targetRouteList.Id));
			}

			var newRouteListItem = new RouteListItem(targetRouteList, order, RouteListItemStatus.EnRoute)
			{
				WithForwarder = targetRouteList.Forwarder != null,
				AddressTransferType = AddressTransferType.FromFreeBalance
			};

			targetRouteList.ObservableAddresses.Add(newRouteListItem);
			targetRouteList.CalculateWages(_wageParameterService);

			newRouteListItem.RecalculateTotalCash();

			if(targetRouteList.ClosingFilled)
			{
				newRouteListItem.FirstFillClosing(_wageParameterService);
			}

			order.ChangeStatus(OrderStatus.OnTheWay);

			unitOfWork.Save(order);
			unitOfWork.Save(newRouteListItem);

			_routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(unitOfWork, newRouteListItem, DeliveryFreeBalanceType.Decrease, needRouteListUpdate: true);

			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, targetRouteList);

			unitOfWork.Save(targetRouteList);
			unitOfWork.Save(targetRouteList.RouteListProfitability);

			return Result.Success(string.Empty);
		}

		private Result<IEnumerable<string>> TransferAddressFrom(
			IUnitOfWork unitOfWork,
			RouteList sourceRouteList,
			RouteList targetRouteList,
			RouteListItem address,
			AddressTransferType? addressTransferType)
		{
			var messages = new List<string>();

			if(addressTransferType != AddressTransferType.NeedToReload
				&& addressTransferType != AddressTransferType.FromHandToHand
				&& addressTransferType != AddressTransferType.FromFreeBalance)
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateTransferTypeNotSet(address.Id, address.Order.DeliveryPoint.ShortAddress));
			}

			if(addressTransferType == AddressTransferType.NeedToReload
				&& targetRouteList.Status >= RouteListStatus.EnRoute)
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateTransferRequiresLoadingWhenRouteListEnRoute(address.Id, address.Order.DeliveryPoint.ShortAddress, targetRouteList.Id));
			}

			if(addressTransferType == AddressTransferType.FromFreeBalance
				&& !_routeListRepository.HasFreeBalanceForOrder(unitOfWork, address.Order, targetRouteList))
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateAddressTransferNotEnoughtFreeBalance(address.Id, targetRouteList.Id));
			}

			if(addressTransferType != AddressTransferType.FromHandToHand
				&& _routeListRepository.IsOrderNeedIndividualSetOnLoad(unitOfWork, address.Order.Id))
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateOrdersWithCreatedUpdNeedToReload(address.Order.Id));
			}

			var transferredAddressFromRouteListTo =
				_routeListItemRepository.GetTransferredRouteListItemFromRouteListForOrder(unitOfWork, targetRouteList.Id, address.Order.Id);

			RouteListItem newAddress;

			if(transferredAddressFromRouteListTo is null)
			{
				newAddress = new RouteListItem(targetRouteList, address.Order, address.Status)
				{
					WasTransfered = true,
					AddressTransferType = addressTransferType,
					WithForwarder = targetRouteList.Forwarder != null
				};

				targetRouteList.ObservableAddresses.Add(newAddress);
				unitOfWork.Save(targetRouteList);

				FlushSessionWithoutCommit(unitOfWork);

				sourceRouteList.TransferAddressTo(unitOfWork, address, newAddress);
				unitOfWork.Save(sourceRouteList);
				unitOfWork.Save(targetRouteList);
			}
			else
			{
				newAddress = transferredAddressFromRouteListTo;
				newAddress.AddressTransferType = addressTransferType;
				address.WasTransfered = false;
				targetRouteList.RevertTransferAddress(_wageParameterService, newAddress, address);
				sourceRouteList.TransferAddressTo(unitOfWork, address, newAddress);
				newAddress.WasTransfered = true;
			}

			if(targetRouteList.Status == RouteListStatus.New)
			{
				if(addressTransferType == AddressTransferType.NeedToReload)
				{
					address.Order.ChangeStatus(OrderStatus.InTravelList);
				}

				if(addressTransferType == AddressTransferType.FromHandToHand)
				{
					address.Order.ChangeStatus(OrderStatus.OnLoading);
				}
			}

			//Пересчёт зарплаты после изменения МЛ

			FlushSessionWithoutCommit(unitOfWork);

			sourceRouteList.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, sourceRouteList);

			targetRouteList.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, targetRouteList);

			address.RecalculateTotalCash();
			newAddress.RecalculateTotalCash();

			if(targetRouteList.ClosingFilled)
			{
				newAddress.FirstFillClosing(_wageParameterService);
			}

			UpdateTransferDocuments(unitOfWork, address, newAddress, addressTransferType.Value);

			if(sourceRouteList.Status == RouteListStatus.Closed)
			{
				messages.AddRange(sourceRouteList.UpdateMovementOperations(_financialCategoriesGroupsSettings));
			}

			if(targetRouteList.Status == RouteListStatus.Closed)
			{
				messages.AddRange(targetRouteList.UpdateMovementOperations(_financialCategoriesGroupsSettings));
			}

			unitOfWork.Save(sourceRouteList);
			unitOfWork.Save(targetRouteList);

			return Result.Success(messages.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		private void FlushSessionWithoutCommit(IUnitOfWork uow)
		{
			// Если транзакция не открыта, то вызов Session.Flush() сразу коммитит изменения в базу

			var transaction = uow.Session.GetCurrentTransaction();

			if(transaction is null)
			{
				uow.Session.BeginTransaction();
			}

			uow.Session.Flush();
		}

		/// <summary>
		/// Возврат переноса
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="sourceRouteList">Маршрутный лист из которого возвращаем перенос</param>
		/// <param name="targetRouteList">Маршрутный лист куда возвращается перенос</param>
		/// <param name="address"></param>
		/// <returns></returns>
		private Result<string> RevertTransferedAddressFrom(
			IUnitOfWork unitOfWork,
			RouteList sourceRouteList,
			RouteList targetRouteList,
			RouteListItem address)
		{
			if(address.Status == RouteListItemStatus.Transfered)
			{
				return Result.Failure<string>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateAlreadyTransfered(address.Id, address.Order.DeliveryPoint.ShortAddress, address.TransferedTo.RouteList.Id));
			}

			RouteListItem pastPlace =
				sourceRouteList?.Addresses?
					.FirstOrDefault(x => x.TransferedTo != null
						&& x.TransferedTo.Id == address.Id)
				?? _routeListItemRepository.GetTransferredFrom(unitOfWork, address);

			var previousRouteList = pastPlace?.RouteList;

			if(pastPlace != null)
			{
				if(pastPlace.TransferedTo.AddressTransferType.Value == AddressTransferType.FromFreeBalance)
				{
					var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(unitOfWork, address.Order, pastPlace.RouteList);

					if(!hasBalanceForTransfer)
					{
						return Result.Failure<string>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.CreateAddressTransferNotEnoughtFreeBalance(address.Id, pastPlace.RouteList.Id));
					}
				}

				var pastPlaceAddressTransferType = pastPlace.TransferedTo.AddressTransferType;

				previousRouteList.RevertTransferAddress(_wageParameterService, pastPlace, address);
				pastPlace.WasTransfered = true;
				pastPlace.AddressTransferType = pastPlaceAddressTransferType;

				UpdateTransferDocuments(unitOfWork, address, pastPlace, pastPlaceAddressTransferType.Value);
				pastPlace.RecalculateTotalCash();
				unitOfWork.Save(pastPlace);
				address.RouteList.TransferAddressTo(unitOfWork, address, pastPlace);
				address.WasTransfered = false;
			}

			address.RouteList.CalculateWages(_wageParameterService);
			address.RecalculateTotalCash();

			return Result.Success(string.Empty);
		}

		private void UpdateTransferDocuments(IUnitOfWork unitOfWork, RouteListItem sourceAddress, RouteListItem targetAddress, AddressTransferType addressTransferType) =>
			_addressTransferController.UpdateDocuments(sourceAddress, targetAddress, unitOfWork, addressTransferType);

		#endregion Transfers - переносы

		public Result ValidateForAccept(
			RouteList routeList,
			IOrderRepository orderRepository,
			bool skipOverfillValidation = false)
		{
			var errors = new List<Error>();

			if(routeList.Car is null)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.CarIsEmpty);
			}

			if(routeList.HasOverweight())
			{
				errors.Add(Vodovoz.Errors.Logistics.RouteList.Overweighted(routeList.Overweight()));
			}

			if(routeList.HasVolumeExecess())
			{
				errors.Add(Vodovoz.Errors.Logistics.RouteList.Overvolumed(routeList.VolumeExecess()));
			}

			if(routeList.HasReverseVolumeExcess())
			{
				errors.Add(Vodovoz.Errors.Logistics.RouteList.InsufficientFreeVolumeForReturn(routeList.ReverseVolumeExecess()));
			}

			var canceledOrdersIds = routeList.Addresses
				.Where(a => orderRepository.GetUndeliveryStatuses().Contains(a.Order.OrderStatus))
				.Select(a => a.Order.Id)
				.ToArray();

			if(canceledOrdersIds.Any())
			{
				errors.Add(Vodovoz.Errors.Logistics.RouteList.ContainsCanceledOrdersOnAccept(canceledOrdersIds));
			}

			var overfillErrorsCodes = Vodovoz.Errors.Logistics.RouteList.OverfilledErrorCodes;

			if(errors.Any()
				&& !(errors.All(error => overfillErrorsCodes.Contains(error.Code))
					&& skipOverfillValidation))
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		private void RecalculateRouteList(IUnitOfWork unitOfWork, RouteList routeList)
		{
			routeList.CalculateWages(_wageParameterService);

			var commonFastDeliveryMaxDistance = (decimal)_deliveryRepository.GetMaxDistanceToLatestTrackPointKmFor(DateTime.Now);
			routeList.UpdateFastDeliveryMaxDistanceValue(commonFastDeliveryMaxDistance);

			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, routeList);
			unitOfWork.Save(routeList.RouteListProfitability);
		}

		public Result<IEnumerable<string>> TryChangeStatusToAccepted(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			Action<bool> disableItemsUpdate,
			IValidator validationService,
			IOrderRepository orderRepository,
			bool skipOverfillValidation = false,
			bool confirmRecalculateRoute = false,
			bool confirmSendOnClosing = false,
			bool confirmSenEnRoute = false)
		{
			var validationResult = ValidateForAccept(routeList, orderRepository, skipOverfillValidation);

			var messages = new List<string>();

			if(validationResult.IsFailure)
			{
				return Result.Failure<IEnumerable<string>>(validationResult.Errors);
			}

			if(routeList.Status != RouteListStatus.New)
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.IncorrectStatusForAccept);
			}

			var contextItems = new Dictionary<object, object>
			{
				{ "NewStatus", RouteListStatus.Confirmed },
				{ nameof(IRouteListItemRepository), _routeListItemRepository }
			};

			var context = new ValidationContext(routeList, null, contextItems);

			if(!validationService.Validate(routeList, context))
			{
				return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.ValidationFailure);
			}

			routeList.ChangeStatusAndCreateTask(RouteListStatus.Confirmed, _callTaskWorker);

			//Строим маршрут для МЛ.
			if((!routeList.PrintsHistory?.Any() ?? true) || confirmRecalculateRoute)
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
						messages.Add($"Для маршрута незапланировано {noPlan} адресов.");
					}
				}
				else
				{
					messages.Add("Маршрут не был перестроен.");
				}
			}

			_logger.LogInformation("Создаём операции по свободным остаткам МЛ {RouteListId}...", routeList.Id);

			foreach(var address in routeList.Addresses)
			{
				if(address.TransferedTo == null &&
				   (!address.WasTransfered || address.AddressTransferType != AddressTransferType.FromHandToHand))
				{
					_routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(
						unitOfWork, address, DeliveryFreeBalanceType.Decrease, isFullRecreation: true, needRouteListUpdate: true);
				}
				else
				{
					_routeListAddressKeepingDocumentController.RemoveRouteListKeepingDocument(unitOfWork, address, true);
				}
			}

			_logger.LogInformation("Операции по свободным остаткакам МЛ {RouteListId} созданы.", routeList.Id);

			if(routeList.GetCarVersion.IsCompanyCar && routeList.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck && !routeList.NeedToLoad)
			{
				if(confirmSendOnClosing)
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
					if(confirmSenEnRoute)
					{
						var contextItemsEnroute = new Dictionary<object, object>
						{
							{ "NewStatus", RouteListStatus.EnRoute },
							{ nameof(IRouteListItemRepository), _routeListItemRepository }
						};

						var contextEnroute = new ValidationContext(routeList, null, contextItemsEnroute);

						if(!validationService.Validate(routeList, contextEnroute))
						{
							return Result.Failure<IEnumerable<string>>(Vodovoz.Errors.Logistics.RouteList.ValidationFailure);
						}

						SendEnRoute(unitOfWork, routeList);
					}
					else
					{
						routeList.ChangeStatusAndCreateTask(RouteListStatus.New, _callTaskWorker);
					}
				}
			}

			RecalculateRouteList(unitOfWork, routeList);

			return Result.Success(messages.AsEnumerable());
		}

		public Result TryChangeStatusToNew(IUnitOfWork unitOfWork, RouteList routeList)
		{
			if(routeList.Status != RouteListStatus.InLoading
				&& routeList.Status != RouteListStatus.Confirmed)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.IncorrectStatusForEdit);
			}

			if(_routeListRepository.GetCarLoadDocuments(unitOfWork, routeList.Id).Any())
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.HasCarLoadingDocuments);
			}

			routeList.ChangeStatusAndCreateTask(RouteListStatus.New, _callTaskWorker);

			RecalculateRouteList(unitOfWork, routeList);

			return Result.Success();
		}

		public Result<RouteListItem> FindTransferTarget(IUnitOfWork unitOfWork, RouteListItem routeListAddress)
		{
			var transferItems = _routeListAddressTransferItemRepository.Get(
				unitOfWork,
				atdi => atdi.NewAddress.Order.Id == routeListAddress.Order.Id)
					.OrderBy(atdi => atdi.Id);

			RouteListItem target = null;

			bool sourceFound = false;

			var sourceTransferItem = transferItems.LastOrDefault(ti => ti.OldAddress.Id == routeListAddress.Id);

			if(sourceTransferItem is null)
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			if(routeListAddress.RecievedTransferAt is null
				&& routeListAddress.Status == RouteListItemStatus.Transfered
				&& transferItems.FirstOrDefault()?.OldAddress.Id != routeListAddress.Id)
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			foreach(var transferItem in transferItems)
			{
				if(!sourceFound)
				{
					if(transferItem.Id != sourceTransferItem.Id)
					{
						continue;
					}

					sourceFound = true;
				}

				if(transferItem.AddressTransferType != AddressTransferType.FromHandToHand)
				{
					break;
				}

				if(transferItem.NewAddress.RecievedTransferAt != null)
				{
					target = transferItem.NewAddress;
					break;
				}

				if(transferItem == transferItems.Last())
				{
					target = transferItem.NewAddress;
					break;
				}

				target = transferItem.NewAddress;
			}

			if(target != null
				&& (target.RouteList.Id == routeListAddress.RouteList.Id
				|| target.Status == RouteListItemStatus.Transfered && target.RecievedTransferAt is null))
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			if(target != null)
			{
				return target;
			}

			return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
		}

		public Result<RouteListItem> FindTransferSource(IUnitOfWork unitOfWork, RouteListItem routeListAddress, bool isFromHandToHandOnly)
		{
			var transferItems = _routeListAddressTransferItemRepository.Get(
				unitOfWork,
				atdi => atdi.OldAddress.Order.Id == routeListAddress.Order.Id)
					.OrderByDescending(atdi => atdi.Id);

			RouteListItem source = null;

			bool targetFound = false;

			var targetTransferItem = transferItems.FirstOrDefault(ti => ti.NewAddress.Id == routeListAddress.Id);

			if(targetTransferItem is null)
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			if(routeListAddress.RecievedTransferAt is null
				&& routeListAddress.Status == RouteListItemStatus.Transfered)
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			foreach(var transferItem in transferItems)
			{
				if(!targetFound)
				{
					if(transferItem.Id != targetTransferItem.Id)
					{
						continue;
					}

					targetFound = true;
				}

				if(isFromHandToHandOnly
					&& transferItem.AddressTransferType != AddressTransferType.FromHandToHand)
				{
					break;
				}

				if(transferItem.OldAddress.RecievedTransferAt != null)
				{
					source = transferItem.OldAddress;
					break;
				}

				if(transferItem == transferItems.Last())
				{
					source = transferItem.OldAddress;
					break;
				}

				source = transferItem.OldAddress;
			}

			if(source != null && source.RouteList.Id == routeListAddress.RouteList.Id)
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			if(source != null)
			{
				return source;
			}

			return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
		}

		public Result<RouteListItem> FindPrevious(IUnitOfWork unitOfWork, RouteListItem routeListAddress)
		{
			var transferItems = _routeListAddressTransferItemRepository.Get(
				unitOfWork,
				atdi => atdi.OldAddress.Order.Id == routeListAddress.Order.Id)
					.OrderByDescending(atdi => atdi.Document.Id);

			var targetTransferItem = transferItems.FirstOrDefault(ti => ti.NewAddress.Id == routeListAddress.Id);

			if(targetTransferItem is null)
			{
				return Result.Failure<RouteListItem>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFound);
			}

			return targetTransferItem.OldAddress;
		}

		public void ConfirmRouteListAddressTransferRecieved(int routeListAddressId, DateTime actionTime)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Подтверждение приема переноса адреса маршрутного листа"))
			{
				var routeListAddress = _routeListAddressesRepository.Get(
					unitOfWork,
					address => address.Id == routeListAddressId)
					.FirstOrDefault();

				routeListAddress.RecievedTransferAt = actionTime;

				unitOfWork.Save(routeListAddress);
				unitOfWork.Commit();
			}
		}
	}
}
