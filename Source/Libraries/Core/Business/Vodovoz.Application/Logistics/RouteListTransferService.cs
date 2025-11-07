using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors.Logistics;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Cash;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Logistics
{
	public class RouteListTransferService : IRouteListTransferService
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IGenericRepository<AddressTransferDocumentItem> _routeListAddressTransferItemRepository;
		private readonly IGenericRepository<RouteListItem> _routeListAddressesRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGenericRepository<RouteListAddressKeepingDocument> _routeListAddressKeepingDocumentsRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly IOnlineOrderService _onlineOrderService;
		private readonly IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IAddressTransferController _addressTransferController;
		private readonly IRouteListService _routeListService;

		public RouteListTransferService(
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IGenericRepository<AddressTransferDocumentItem> routeListAddressTransferItemRepository,
			IGenericRepository<RouteListItem> routeListAddressesRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<RouteListAddressKeepingDocument> routeListAddressKeepingDocumentsRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			IOnlineOrderService onlineOrderService,
			IWageParameterService wageParameterService,
			IRouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IAddressTransferController addressTransferController,
			IRouteListService routeListService)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListAddressTransferItemRepository = routeListAddressTransferItemRepository ??
			                                          throw new ArgumentNullException(nameof(routeListAddressTransferItemRepository));
			_routeListAddressesRepository = routeListAddressesRepository ?? throw new ArgumentNullException(nameof(routeListAddressesRepository));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListAddressKeepingDocumentsRepository = routeListAddressKeepingDocumentsRepository ??
			                                              throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentsRepository));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_onlineOrderService = onlineOrderService ?? throw new ArgumentNullException(nameof(onlineOrderService));
			_routeListAddressKeepingDocumentController = routeListAddressKeepingDocumentController ??
			                                             throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentController));
			_financialCategoriesGroupsSettings =
				financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_addressTransferController = addressTransferController ?? throw new ArgumentNullException(nameof(addressTransferController));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
		}

		public Result<IEnumerable<string>> TransferAddressesFrom(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
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
				return Result.Failure<IEnumerable<string>>(RouteListErrors.CreateNotFound(targetRouteListId));
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
				var result = TransferAddressFrom(unitOfWork, wageParameterService, sourceRouteList, targetRouteList, address, addressIdsAndTransferType[address.Id]);

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

		public Result<IEnumerable<string>> RevertTransferedAddressesFrom(IUnitOfWork unitOfWork,
			int sourceRouteListId,
			int? targetRouteListId,
			IEnumerable<int> addressIds, IWageParameterService wageParameterService)
		{
			var messages = new List<string>();
			var errors = new List<string>();

			var sourceRouteList = unitOfWork.Session
				.Query<RouteList>()
				.Where(x => x.Id == sourceRouteListId)
				.FirstOrDefault();

			if(sourceRouteList is null)
			{
				return Result.Failure<IEnumerable<string>>(RouteListErrors.CreateNotFound(sourceRouteListId));
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
				var result = RevertTransferedAddressFrom(unitOfWork, wageParameterService, sourceRouteList, targetRouteList, address);

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

			sourceRouteList.CalculateWages(wageParameterService);

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

		private Result<string> TransferOrderTo(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
			RouteList targetRouteList,
			Order order,
			AddressTransferType? transferType)
		{
			if(transferType != AddressTransferType.FromFreeBalance)
			{
				return Result.Failure<string>(RouteListErrors.RouteListItem.CreateInvalidOrderTransferType(order.Id));
			}

			var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(unitOfWork, order, targetRouteList);

			if(!hasBalanceForTransfer)
			{
				return Result.Failure<string>(
					RouteListErrors.RouteListItem.CreateOrderTransferNotEnoughtFreeBalance(order.Id, targetRouteList.Id));
			}

			var newRouteListItem = new RouteListItem(targetRouteList, order, RouteListItemStatus.EnRoute)
			{
				WithForwarder = targetRouteList.Forwarder != null,
				AddressTransferType = AddressTransferType.FromFreeBalance
			};

			_onlineOrderService.NotifyClientOfOnlineOrderStatusChange(unitOfWork, order.OnlineOrder);

			targetRouteList.ObservableAddresses.Add(newRouteListItem);
			targetRouteList.CalculateWages(wageParameterService);

			newRouteListItem.RecalculateTotalCash();

			if(targetRouteList.ClosingFilled)
			{
				newRouteListItem.FirstFillClosing(wageParameterService);
			}

			order.ChangeStatus(OrderStatus.OnTheWay);

			unitOfWork.Save(order);
			unitOfWork.Save(newRouteListItem);

			_routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(unitOfWork, newRouteListItem,
				DeliveryFreeBalanceType.Decrease, needRouteListUpdate: true);

			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, targetRouteList);

			unitOfWork.Save(targetRouteList);
			unitOfWork.Save(targetRouteList.RouteListProfitability);

			return Result.Success(string.Empty);
		}

		private Result<IEnumerable<string>> TransferAddressFrom(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
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
				return Result.Failure<IEnumerable<string>>(
					RouteListErrors.RouteListItem.CreateTransferTypeNotSet(address.Id,
						address.Order.DeliveryPoint.ShortAddress));
			}

			if(addressTransferType == AddressTransferType.NeedToReload
			   && targetRouteList.Status >= RouteListStatus.EnRoute)
			{
				return Result.Failure<IEnumerable<string>>(
					RouteListErrors.RouteListItem.CreateTransferRequiresLoadingWhenRouteListEnRoute(address.Id,
						address.Order.DeliveryPoint.ShortAddress, targetRouteList.Id));
			}

			if(addressTransferType == AddressTransferType.FromFreeBalance
			   && !_routeListRepository.HasFreeBalanceForOrder(unitOfWork, address.Order, targetRouteList))
			{
				return Result.Failure<IEnumerable<string>>(
					RouteListErrors.RouteListItem
						.CreateAddressTransferNotEnoughtFreeBalance(address.Id, targetRouteList.Id));
			}

			if(addressTransferType != AddressTransferType.FromHandToHand
			   && _routeListRepository.IsOrderNeedIndividualSetOnLoad(unitOfWork, address.Order.Id))
			{
				return Result.Failure<IEnumerable<string>>(
					RouteListErrors.RouteListItem.CreateOrdersWithCreatedUpdNeedToReload(address.Order.Id));
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

				TransferAddressTo(unitOfWork, sourceRouteList, address, newAddress);
				unitOfWork.Save(sourceRouteList);
				unitOfWork.Save(targetRouteList);
			}
			else
			{
				newAddress = transferredAddressFromRouteListTo;
				newAddress.AddressTransferType = addressTransferType;
				address.WasTransfered = false;
				RevertTransferAddress(unitOfWork, targetRouteList, newAddress, address, wageParameterService);
				TransferAddressTo(unitOfWork, sourceRouteList, address, newAddress);
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

			sourceRouteList.CalculateWages(wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, sourceRouteList);

			targetRouteList.CalculateWages(wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, targetRouteList);

			address.RecalculateTotalCash();
			newAddress.RecalculateTotalCash();

			if(targetRouteList.ClosingFilled)
			{
				newAddress.FirstFillClosing(wageParameterService);
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

		public Result<IEnumerable<string>> TransferOrdersTo(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
			int targetRouteListId,
			IDictionary<int, AddressTransferType?> ordersIdsAndTransferType)
		{
			var messages = new List<string>();

			var errors = new List<Error>();

			var targetRouteList = unitOfWork.GetById<RouteList>(targetRouteListId);

			if(targetRouteList is null)
			{
				return Result.Failure<IEnumerable<string>>(RouteListErrors.CreateNotFound(targetRouteListId));
			}

			var ordersToTransfer = unitOfWork.Session.Query<Order>()
				.Where(x => ordersIdsAndTransferType.Keys.Contains(x.Id));

			foreach(var order in ordersToTransfer)
			{
				var result = TransferOrderTo(unitOfWork, wageParameterService, targetRouteList, order, ordersIdsAndTransferType[order.Id]);

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
		/// <param name="wageParameterService"></param>
		/// <param name="sourceRouteList">Маршрутный лист из которого возвращаем перенос</param>
		/// <param name="targetRouteList">Маршрутный лист куда возвращается перенос</param>
		/// <param name="address"></param>
		/// <returns></returns>
		private Result<string> RevertTransferedAddressFrom(IUnitOfWork unitOfWork,
			IWageParameterService wageParameterService,
			RouteList sourceRouteList,
			RouteList targetRouteList,
			RouteListItem address)
		{
			if(address.Status == RouteListItemStatus.Transfered)
			{
				return Result.Failure<string>(RouteListErrors.RouteListItem.CreateAlreadyTransfered(address.Id,
					address.Order.DeliveryPoint.ShortAddress, address.TransferedTo.RouteList.Id));
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
						return Result.Failure<string>(
							RouteListErrors.RouteListItem.CreateAddressTransferNotEnoughtFreeBalance(address.Id,
								pastPlace.RouteList.Id));
					}
				}

				var pastPlaceAddressTransferType = pastPlace.TransferedTo.AddressTransferType;

				RevertTransferAddress(unitOfWork, previousRouteList, pastPlace, address, wageParameterService);
				pastPlace.WasTransfered = true;
				pastPlace.AddressTransferType = pastPlaceAddressTransferType;

				UpdateTransferDocuments(unitOfWork, address, pastPlace, pastPlaceAddressTransferType.Value);
				pastPlace.RecalculateTotalCash();
				unitOfWork.Save(pastPlace);
				TransferAddressTo(unitOfWork, address.RouteList, address, pastPlace);
				address.WasTransfered = false;
			}

			address.RouteList.CalculateWages(wageParameterService);
			address.RecalculateTotalCash();

			return Result.Success(string.Empty);
		}

		public void TransferAddressTo(IUnitOfWork unitOfWork, RouteList routeList, RouteListItem transferringAddress, RouteListItem targetAddress)
		{
			transferringAddress.TransferTo(unitOfWork, targetAddress);
			_routeListService.UpdateStatus(unitOfWork, routeList);
		}

		public void RevertTransferAddress(IUnitOfWork unitOfWork, RouteList routeList,
			RouteListItem targetAddress, RouteListItem revertedAddress, IWageParameterService wageParameterService)
		{
			targetAddress.RevertTransferAddress(unitOfWork, wageParameterService, revertedAddress);

			_routeListService.UpdateStatus(unitOfWork, routeList);
		}

		private void UpdateTransferDocuments(IUnitOfWork unitOfWork, RouteListItem sourceAddress, RouteListItem targetAddress,
			AddressTransferType addressTransferType) =>
			_addressTransferController.UpdateDocuments(sourceAddress, targetAddress, unitOfWork, addressTransferType);


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
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
			}

			if(routeListAddress.RecievedTransferAt is null
			   && routeListAddress.Status == RouteListItemStatus.Transfered
			   && transferItems.FirstOrDefault()?.OldAddress.Id != routeListAddress.Id)
			{
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
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
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
			}

			if(target != null)
			{
				return target;
			}

			return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
		}

		public Result<RouteListItem> FindTransferSource(IUnitOfWork unitOfWork, RouteListItem routeListAddress)
		{
			var transferItemsWithOldAddress = _routeListAddressTransferItemRepository.Get(
					unitOfWork,
					atdi => atdi.OldAddress.Order.Id == routeListAddress.Order.Id)
				.OrderByDescending(atdi => atdi.Id);

			RouteListItem source = null;

			bool targetWithNewAddressFound = false;

			var lastTargetTransferItemWithNewAddress = transferItemsWithOldAddress.FirstOrDefault(ti => ti.NewAddress.Id == routeListAddress.Id);

			if(lastTargetTransferItemWithNewAddress is null)
			{
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
			}

			if(routeListAddress.RecievedTransferAt is null
			   && routeListAddress.Status == RouteListItemStatus.Transfered)
			{
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
			}

			foreach(var transferItemWithOldAddress in transferItemsWithOldAddress)
			{
				if(!targetWithNewAddressFound)
				{
					if(transferItemWithOldAddress.Id != lastTargetTransferItemWithNewAddress.Id)
					{
						continue;
					}

					targetWithNewAddressFound = true;
				}

				if(transferItemWithOldAddress.AddressTransferType != AddressTransferType.FromHandToHand)
				{
					break;
				}

				if(transferItemWithOldAddress.OldAddress.RecievedTransferAt != null)
				{
					source = transferItemWithOldAddress.OldAddress;
					break;
				}

				if(transferItemWithOldAddress == transferItemsWithOldAddress.Last())
				{
					source = transferItemWithOldAddress.OldAddress;
					break;
				}

				source = transferItemWithOldAddress.OldAddress;
			}

			if(source != null && source.RouteList.Id == routeListAddress.RouteList.Id)
			{
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
			}

			if(source != null)
			{
				return source;
			}

			return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
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
				return Result.Failure<RouteListItem>(RouteListErrors.RouteListItem.NotFound);
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
