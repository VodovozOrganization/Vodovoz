using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Osrm;
using QS.Services;
using QS.Validation;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Errors.Logistics;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Logistics
{
	public class RouteListService : IRouteListService
	{
		private readonly ILogger<RouteListService> _logger;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IUserService _userService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IPermissionRepository _permissionRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListSpecialConditionsService _routeListSpecialConditionsService;
		private readonly IOnlineOrderService _onlineOrderService;
		private readonly IOrderService _orderService;
		private readonly IOsrmSettings _osrmSettings;
		private readonly IOsrmClient _osrmClient;

		public RouteListService(
			ILogger<RouteListService> logger,
			IRouteListRepository routeListRepository,
			IDeliveryRepository deliveryRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			INomenclatureSettings nomenclatureSettings,
			IOrderRepository orderRepository,
			IUserService userService,
			ISubdivisionRepository subdivisionRepository,
			IPermissionRepository permissionRepository,
			IEmployeeRepository employeeRepository,
			ITrackRepository trackRepository,
			IRouteListSpecialConditionsService routeListSpecialConditionsService,
			IOnlineOrderService onlineOrderService,
			IOrderService orderService,
			IOsrmSettings osrmSettings,
			IOsrmClient osrmClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListSpecialConditionsService =
				routeListSpecialConditionsService ?? throw new ArgumentNullException(nameof(routeListSpecialConditionsService));
			_onlineOrderService = onlineOrderService ?? throw new ArgumentNullException(nameof(onlineOrderService));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_osrmSettings = osrmSettings ?? throw new ArgumentNullException(nameof(osrmSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
		}

		#region Статусы МЛ

		public bool TrySendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			ICallTaskWorker callTaskWorker,
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
					SendEnRoute(unitOfWork, routeList, callTaskWorker);
				}
			}

			return closed;
		}

		public void SendEnRoute(
			IUnitOfWork unitOfWork,
			int routeListId,
			ICallTaskWorker callTaskWorker)
		{
			using(var transaction = unitOfWork.Session.BeginTransaction())
			{
				var routeList = _routeListRepository.GetRouteListById(unitOfWork, routeListId);

				if(routeList is null)
				{
					_logger.LogWarning("Маршрутный лист с номером {RouteListId} не найден, не удалось отправить в путь", routeListId);

					return;
				}

				SendEnRoute(unitOfWork, routeList, callTaskWorker);

				transaction.Commit();
			}
		}

		public void SendEnRoute(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			ICallTaskWorker callTaskWorker)
		{
			if(routeList is null)
			{
				_logger.LogWarning("Маршрутный лист с номером {RouteListId} не найден, не удалось отправить в путь", routeList.Id);

				return;
			}

			if(!routeList.SpecialConditionsAccepted)
			{
				_logger.LogTrace("Специальыне условия не приняты. Добавление отсутствующих");

				_routeListSpecialConditionsService.CreateSpecialConditionsFor(unitOfWork, routeList);
			}

			ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.EnRoute, callTaskWorker);
		}

		public Result TryChangeStatusToNew(IUnitOfWork unitOfWork, RouteList routeList, IWageParameterService wageParameterService, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.InLoading
			   && routeList.Status != RouteListStatus.Confirmed)
			{
				return Result.Failure(RouteListErrors.IncorrectStatusForEdit);
			}

			if(_routeListRepository.GetCarLoadDocuments(unitOfWork, routeList.Id).Any())
			{
				return Result.Failure(RouteListErrors.HasCarLoadingDocuments);
			}

			ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.New, callTaskWorker);

			RecalculateRouteList(unitOfWork, routeList, wageParameterService);

			return Result.Success();
		}

		public void CompleteRoute(IUnitOfWork unitOfWork, RouteList routeList, IWageParameterService wageParameterService, ICallTaskWorker callTaskWorker)
		{
			ChangeStatus(unitOfWork, routeList, RouteListStatus.Delivered);

			var track = _trackRepository.GetTrackByRouteListId(unitOfWork, routeList.Id);
			if(track != null)
			{
				track.CalculateDistance();
				track.CalculateDistanceToBase(_osrmSettings, _osrmClient);
				unitOfWork.Save(track);
			}

			routeList.FirstFillClosing(wageParameterService);
			unitOfWork.Save(routeList);
		}

		public void CompleteRouteAndCreateTask(
			IUnitOfWork unitOfWork,
			RouteList routeList,
			IWageParameterService wageParameterService,
			ICallTaskWorker callTaskWorker)
		{
			if(routeList.NeedMileageCheck)
			{
				ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.MileageCheck, callTaskWorker);
			}
			else
			{
				ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.OnClosing, callTaskWorker);
			}

			var track = _trackRepository.GetTrackByRouteListId(unitOfWork, routeList.Id);
			if(track != null)
			{
				track.CalculateDistance();
				track.CalculateDistanceToBase(_osrmSettings, _osrmClient);
				unitOfWork.Save(track);
			}

			routeList.FirstFillClosing(wageParameterService);
			unitOfWork.Save(routeList);
		}

		public Result AcceptCash(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.OnClosing)
			{
				return Result.Failure(RouteListErrors.IncorrectStatusForClose());
			}

			if(routeList.Cashier == null)
			{
				return Result.Failure(RouteListErrors.CashierIsEmpty);
			}

			return ConfirmAndClose(unitOfWork, routeList, callTaskWorker);
		}

		public bool AcceptMileage(IUnitOfWork unitOfWork, RouteList routeList, IValidator validator, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.MileageCheck)
			{
				return true;
			}

			routeList.RecalculateFuelOutlay();

			if(!routeList.TryValidateFuelOperation(validator))
			{
				return false;
			}

			ConfirmAndClose(unitOfWork, routeList, callTaskWorker);
			return true;
		}

		public void ChangeStatus(IUnitOfWork unitOfWork, RouteList routeList, RouteListStatus newStatus)
		{
			if(newStatus == routeList.Status)
			{
				return;
			}

			string exceptionMessage = $"Некорректная операция.!! Не предусмотрена смена статуса с {routeList.Status} на {newStatus}";

			switch(newStatus)
			{
				case RouteListStatus.New:
					if(routeList.Status == RouteListStatus.Confirmed || routeList.Status == RouteListStatus.InLoading)
					{
						routeList.Status = RouteListStatus.New;
						foreach(var address in routeList.Addresses)
						{
							if(address.Order.OrderStatus == OrderStatus.OnLoading)
							{
								address.Order.ChangeStatus(OrderStatus.InTravelList);
							}
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.Confirmed:
					if(routeList.Status == RouteListStatus.New || routeList.Status == RouteListStatus.InLoading)
					{
						routeList.Status = RouteListStatus.Confirmed;
						foreach(var address in routeList.Addresses)
						{
							if(address.Order.OrderStatus < OrderStatus.OnLoading)
							{
								address.Order.ChangeStatus(OrderStatus.OnLoading);
							}
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.InLoading:
					if(routeList.Status == RouteListStatus.EnRoute)
					{
						routeList.Status = RouteListStatus.InLoading;
						foreach(var item in routeList.Addresses)
						{
							if(item.Order.OrderStatus != OrderStatus.OnLoading)
							{
								item.Order.ChangeStatus(OrderStatus.OnLoading);
							}
						}
					}
					else if(routeList.Status == RouteListStatus.Confirmed)
					{
						routeList.Status = RouteListStatus.InLoading;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.EnRoute:
					if(routeList.Status == RouteListStatus.InLoading
					   || routeList.Status == RouteListStatus.Confirmed
					   || routeList.Status == RouteListStatus.Delivered)
					{
						foreach(var item in routeList.Addresses)
						{
							bool isInvalidStatus = _orderRepository.GetUndeliveryStatuses().Contains(item.Order.OrderStatus);

							if(!isInvalidStatus)
							{
								item.Order.OrderStatus = OrderStatus.OnTheWay;
								_onlineOrderService.NotifyClientOfOnlineOrderStatusChange(unitOfWork, item.Order.OnlineOrder);
							}
						}

						routeList.Status = RouteListStatus.EnRoute;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.Delivered:
					if(routeList.Status == RouteListStatus.EnRoute)
					{
						routeList.DeliveredAt = DateTime.Now;
						routeList.Status = newStatus;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.OnClosing:
					if(
						(routeList.Status == RouteListStatus.EnRoute
						 && (routeList.GetCarVersion.CarOwnType == CarOwnType.Company && routeList.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck
						     || routeList.Driver.VisitingMaster || !routeList.NeedMileageCheckByWage))
						|| (routeList.Status == RouteListStatus.Confirmed && (routeList.GetCarVersion.CarOwnType == CarOwnType.Company &&
						                                                      routeList.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck))
						|| routeList.Status == RouteListStatus.MileageCheck
						|| routeList.Status == RouteListStatus.Delivered
						|| routeList.Status == RouteListStatus.Closed)
					{
						if(routeList.DeliveredAt is null)
						{
							routeList.DeliveredAt = DateTime.Now;
						}

						routeList.Status = newStatus;
						foreach(var item in routeList.Addresses.Where(x =>
							        x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute))
						{
							item.Order.ChangeStatus(OrderStatus.UnloadingOnStock);
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.MileageCheck:
					if(routeList.Status == RouteListStatus.Delivered || routeList.Status == RouteListStatus.OnClosing)
					{
						routeList.Status = newStatus;
						foreach(var item in routeList.Addresses.Where(x =>
							        x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute))
						{
							item.Order.ChangeStatus(OrderStatus.UnloadingOnStock);
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.Closed:
					if(routeList.Status == RouteListStatus.OnClosing
					   || routeList.Status == RouteListStatus.MileageCheck
					   || routeList.Status == RouteListStatus.Delivered)
					{
						routeList.Status = newStatus;
						CloseAddresses(unitOfWork, routeList);
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				default:
					throw new NotImplementedException($"Не реализовано изменение статуса для {newStatus}");
			}

			routeList.UpdateDeliveryDocuments(unitOfWork);
			routeList.UpdateClosedInformation();
		}

		public void ChangeStatusAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, RouteListStatus newStatus, ICallTaskWorker callTaskWorker)
		{
			if(newStatus == routeList.Status)
			{
				return;
			}

			string exceptionMessage = $"Некорректная операция!. Не предусмотрена смена статуса с {routeList.Status} на {newStatus}";

			switch(newStatus)
			{
				case RouteListStatus.New:
					if(routeList.Status == RouteListStatus.Confirmed || routeList.Status == RouteListStatus.InLoading)
					{
						routeList.Status = RouteListStatus.New;
						foreach(var address in routeList.Addresses)
						{
							if(address.Order.OrderStatus == OrderStatus.OnLoading)
							{
								address.Order.ChangeStatusAndCreateTasks(OrderStatus.InTravelList, callTaskWorker);
							}
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.Confirmed:
					if(routeList.Status == RouteListStatus.New || routeList.Status == RouteListStatus.InLoading)
					{
						routeList.Status = RouteListStatus.Confirmed;
						foreach(var address in routeList.Addresses)
						{
							if(address.Order.OrderStatus < OrderStatus.OnLoading)
							{
								address.Order.ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);
							}
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.InLoading:
					if(routeList.Status == RouteListStatus.EnRoute)
					{
						routeList.Status = RouteListStatus.InLoading;
						foreach(var item in routeList.Addresses)
						{
							if(item.Order.OrderStatus != OrderStatus.OnLoading)
							{
								item.Order.ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);
							}
						}
					}
					else if(routeList.Status == RouteListStatus.Confirmed)
					{
						routeList.Status = RouteListStatus.InLoading;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.EnRoute:
					if(routeList.Status == RouteListStatus.InLoading || routeList.Status == RouteListStatus.Confirmed
					                                                 || routeList.Status == RouteListStatus.Delivered)
					{
						if(routeList.Status != RouteListStatus.Delivered)
						{
							foreach(var address in routeList.Addresses)
							{
								if(address.Status == RouteListItemStatus.Transfered)
								{
									continue;
								}

								bool isInvalidStatus = _orderRepository.GetUndeliveryStatuses().Contains(address.Order.OrderStatus);

								if(!isInvalidStatus)
								{
									address.Order.OrderStatus = OrderStatus.OnTheWay;
									_onlineOrderService.NotifyClientOfOnlineOrderStatusChange(unitOfWork, address.Order.OnlineOrder);
								}
							}
						}

						routeList.Status = RouteListStatus.EnRoute;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.Delivered:
					if(routeList.Status == RouteListStatus.EnRoute)
					{
						routeList.Status = newStatus;
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.OnClosing:
					if(
						(routeList.Status == RouteListStatus.Delivered
						 && (routeList.GetCarVersion.CarOwnType == CarOwnType.Company && routeList.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck
						     || routeList.Driver.VisitingMaster || !routeList.NeedMileageCheckByWage))
						|| (routeList.Status == RouteListStatus.Confirmed
						    && (routeList.GetCarVersion.CarOwnType == CarOwnType.Company &&
						        routeList.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck))
						|| routeList.Status == RouteListStatus.MileageCheck || routeList.Status == RouteListStatus.Delivered
						|| routeList.Status == RouteListStatus.Closed)
					{
						routeList.Status = newStatus;
						foreach(var item in routeList.Addresses.Where(x =>
							        x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute))
						{
							item.Order.ChangeStatusAndCreateTasks(OrderStatus.UnloadingOnStock, callTaskWorker);
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.MileageCheck:
					if(routeList.Status == RouteListStatus.Delivered || routeList.Status == RouteListStatus.OnClosing)
					{
						routeList.Status = newStatus;
						foreach(var item in routeList.Addresses.Where(x =>
							        x.Status == RouteListItemStatus.Completed || x.Status == RouteListItemStatus.EnRoute))
						{
							item.Order.ChangeStatusAndCreateTasks(OrderStatus.UnloadingOnStock, callTaskWorker);
						}
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				case RouteListStatus.Closed:
					if(routeList.Status == RouteListStatus.OnClosing
					   || routeList.Status == RouteListStatus.MileageCheck
					   || routeList.Status == RouteListStatus.Delivered)
					{
						routeList.Status = newStatus;
						CloseAddressesAndCreateTask(unitOfWork, routeList, callTaskWorker);
					}
					else
					{
						throw new InvalidOperationException(exceptionMessage);
					}

					break;
				default:
					throw new NotImplementedException($"Не реализовано изменение статуса для {newStatus}");
			}

			routeList.UpdateDeliveryDocuments(unitOfWork);
			routeList.UpdateClosedInformation();
		}

		private void RecalculateRouteList(IUnitOfWork unitOfWork, RouteList routeList, IWageParameterService wageParameterService)
		{
			routeList.CalculateWages(wageParameterService);

			var commonFastDeliveryMaxDistance = (decimal)_deliveryRepository.GetMaxDistanceToLatestTrackPointKmFor(DateTime.Now);
			routeList.UpdateFastDeliveryMaxDistanceValue(commonFastDeliveryMaxDistance);

			_routeListProfitabilityController.ReCalculateRouteListProfitability(unitOfWork, routeList);
			unitOfWork.Save(routeList.RouteListProfitability);
		}

		private Result ConfirmAndClose(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.OnClosing && routeList.Status != RouteListStatus.MileageCheck)
			{
				return Result.Failure(RouteListErrors.IncorrectStatusForClose(
					"Закрыть маршрутный лист можно только если он находится в статусе {RouteListStatus.OnClosing} или  {RouteListStatus.MileageCheck}"));
			}

			if(routeList.Driver != null && routeList.Driver.FirstWorkDay == null)
			{
				routeList.Driver.FirstWorkDay = routeList.Date;
				unitOfWork.Save(routeList.Driver);
			}

			if(routeList.Forwarder != null && routeList.Forwarder.FirstWorkDay == null)
			{
				routeList.Forwarder.FirstWorkDay = routeList.Date;
				unitOfWork.Save(routeList.Forwarder);
			}

			switch(routeList.Status)
			{
				case RouteListStatus.OnClosing:
					CloseFromOnClosing(unitOfWork, routeList, callTaskWorker);
					break;
				case RouteListStatus.MileageCheck:
					CloseFromOnMileageCheck(unitOfWork, routeList, callTaskWorker);
					break;
			}

			return Result.Success();
		}

		private void CloseFromOnMileageCheck(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.MileageCheck)
			{
				return;
			}

			if(routeList.WasAcceptedByCashier && routeList.IsConsistentWithUnloadDocument() && !routeList.HasMoneyDiscrepancy)
			{
				ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.Closed, callTaskWorker);
			}
			else
			{
				ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.OnClosing, callTaskWorker);
			}
		}

		/// <summary>
		/// Закрывает МЛ, либо переводит в проверку км, при необходимых условиях, из статуса "Сдается" 
		/// </summary>
		private void CloseFromOnClosing(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.OnClosing)
			{
				return;
			}

			if((!routeList.NeedMileageCheck || (routeList.NeedMileageCheck && routeList.ConfirmedDistance > 0))
			   && routeList.IsConsistentWithUnloadDocument()
			   && _permissionRepository.HasAccessToClosingRoutelist(unitOfWork, _subdivisionRepository, _employeeRepository, _userService))
			{
				ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.Closed, callTaskWorker);
				return;
			}

			if(routeList.NeedMileageCheck && routeList.ConfirmedDistance <= 0)
			{
				ChangeStatusAndCreateTask(unitOfWork, routeList, RouteListStatus.MileageCheck, callTaskWorker);
				return;
			}
		}

		public void UpdateStatus(IUnitOfWork unitOfWork, RouteList routeList, bool isIgnoreAdditionalLoadingDocument = false)
		{
			if(isIgnoreAdditionalLoadingDocument
				   ? routeList.CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument
				   : routeList.CanChangeStatusToDelivered)
			{
				ChangeStatus(unitOfWork, routeList, RouteListStatus.Delivered);
			}
		}

		public Result ValidateForAccept(
			RouteList routeList,
			IOrderRepository orderRepository,
			bool skipOverfillValidation = false)
		{
			var errors = new List<Error>();

			if(routeList.Car is null)
			{
				return Result.Failure(RouteListErrors.CarIsEmpty);
			}

			if(routeList.HasOverweight())
			{
				errors.Add(RouteListErrors.Overweighted(routeList.Overweight()));
			}

			if(routeList.HasVolumeExecess())
			{
				errors.Add(RouteListErrors.Overvolumed(routeList.VolumeExecess()));
			}

			if(routeList.HasReverseVolumeExcess())
			{
				errors.Add(RouteListErrors.InsufficientFreeVolumeForReturn(routeList.ReverseVolumeExecess()));
			}

			var canceledOrdersIds = routeList.Addresses
				.Where(a => orderRepository.GetUndeliveryStatuses().Contains(a.Order.OrderStatus))
				.Select(a => a.Order.Id)
				.ToArray();

			if(canceledOrdersIds.Any())
			{
				errors.Add(RouteListErrors.ContainsCanceledOrdersOnAccept(canceledOrdersIds));
			}

			var overfillErrorsCodes = RouteListErrors.OverfilledErrorCodes;

			if(errors.Any()
			   && !(errors.All(error => overfillErrorsCodes.Contains(error.Code))
			        && skipOverfillValidation))
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		#endregion Статусы МЛ


		#region Адреса в МЛ

		public RouteListItem AddAddressFromOrder(IUnitOfWork unitOfWork, RouteList routeList, Order order)
		{
			if(order == null) throw new ArgumentNullException(nameof(order));

			if(order.DeliveryPoint == null)
				throw new NullReferenceException("В маршрутный нельзя добавить заказ без точки доставки.");
			var item = new RouteListItem(routeList, order, RouteListItemStatus.EnRoute)
			{
				WithForwarder = routeList.Forwarder != null
			};

			routeList.ObservableAddresses.Add(item);

			return item;
		}

		public RouteListItem AddAddressFromOrder(IUnitOfWork unitOfWork, RouteList routeList, int orderId)
		{
			var order = unitOfWork.GetById<Order>(orderId);
			if(order == null)
			{
				throw new NullReferenceException($"Ошибка добавления заказа в маршрутный лист. Заказ с номером {orderId} не найден.");
			}

			if(order.DeliveryPoint == null)
				throw new NullReferenceException("В маршрутный нельзя добавить заказ без точки доставки.");
			var item = new RouteListItem(routeList, order, RouteListItemStatus.EnRoute)
			{
				WithForwarder = routeList.Forwarder != null
			};

			routeList.ObservableAddresses.Add(item);
			return item;
		}

		public void ChangeAddressStatus(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid, RouteListItemStatus newAddressStatus, 
			ICallTaskWorker callTaskWorker)
		{
			UpdateStatus(unitOfWork, routeList.Addresses.First(a => a.Id == routeListAddressid), newAddressStatus);
			UpdateStatus(unitOfWork, routeList);
		}

		public void ChangeAddressStatusAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, int routeListAddressid,
			RouteListItemStatus newAddressStatus, ICallTaskWorker callTaskWorker, bool isEditAtCashier = false)
		{
			routeList.Addresses.First(a => a.Id == routeListAddressid)
				.UpdateStatusAndCreateTask(unitOfWork, newAddressStatus, callTaskWorker, _onlineOrderService, isEditAtCashier);
			UpdateStatus(unitOfWork, routeList);
		}

		public void SetAddressStatusWithoutOrderChange(IUnitOfWork unitOfWork, RouteList routeList, RouteListItem routeListAddress,
			RouteListItemStatus newAddressStatus, bool needCreateDeliveryFreeBalanceOperation = true)
		{
			if(routeList is null || routeListAddress is null)
			{
				return;
			}

			routeList.Addresses.First(a => a.Id == routeListAddress.Id)
				.SetStatusWithoutOrderChange(unitOfWork, newAddressStatus, needCreateDeliveryFreeBalanceOperation);
			UpdateStatus(unitOfWork, routeList);
		}


		public void UpdateStatus(IUnitOfWork uow, RouteListItem address, RouteListItemStatus status)
		{
			if(address.Status == status)
			{
				return;
			}

			var oldStatus = address.Status;
			address.Status = status;
			address.StatusLastUpdate = DateTime.Now;

			switch(address.Status)
			{
				case RouteListItemStatus.Canceled:
					address.Order.ChangeStatus(OrderStatus.DeliveryCanceled);
					address.SetOrderActualCountsToZeroOnCanceled();
					break;
				case RouteListItemStatus.Completed:
					address.Order.ChangeStatus(OrderStatus.Shipped);

					if(address.Order.TimeDelivered == null)
					{
						address.Order.TimeDelivered = DateTime.Now;
					}

					address.RestoreOrder();
					_orderService.AutoCancelAutoTransfer(uow, address.Order);
					break;
				case RouteListItemStatus.EnRoute:
					address.Order.ChangeStatus(OrderStatus.OnTheWay);
					address.RestoreOrder();
					_onlineOrderService.NotifyClientOfOnlineOrderStatusChange(uow, address.Order.OnlineOrder);
					break;
				case RouteListItemStatus.Overdue:
					address.Order.ChangeStatus(OrderStatus.NotDelivered);
					address.SetOrderActualCountsToZeroOnCanceled();
					break;
			}

			uow.Save(address.Order);

			address.CreateDeliveryFreeBalanceOperation(uow, oldStatus, status);

			address.UpdateRouteListDebt();
		}

		public void CloseAddresses(IUnitOfWork unitOfWork, RouteList routeList)
		{
			if(routeList.Status != RouteListStatus.Closed)
			{
				return;
			}

			foreach(var address in routeList.Addresses)
			{
				if(address.Status == RouteListItemStatus.Completed || address.Status == RouteListItemStatus.EnRoute)
				{
					if(address.Status == RouteListItemStatus.EnRoute)
					{
						UpdateStatus(unitOfWork, address, RouteListItemStatus.Completed);
					}

					address.Order.ChangeStatus(OrderStatus.Closed);
				}

				if(address.Status == RouteListItemStatus.Canceled)
				{
					address.Order.ChangeStatus(OrderStatus.DeliveryCanceled);
				}

				if(address.Status == RouteListItemStatus.Overdue)
				{
					address.Order.ChangeStatus(OrderStatus.NotDelivered);
				}
			}
		}

		public void CloseAddressesAndCreateTask(IUnitOfWork unitOfWork, RouteList routeList, ICallTaskWorker callTaskWorker)
		{
			if(routeList.Status != RouteListStatus.Closed)
			{
				return;
			}

			foreach(var address in routeList.Addresses)
			{
				if(address.Status == RouteListItemStatus.Completed || address.Status == RouteListItemStatus.EnRoute)
				{
					if(address.Status == RouteListItemStatus.EnRoute)
					{
						address.UpdateStatusAndCreateTask(unitOfWork, RouteListItemStatus.Completed, callTaskWorker, _onlineOrderService);
					}

					address.Order.ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
				}

				if(address.Status == RouteListItemStatus.Canceled)
				{
					address.Order.ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
				}

				if(address.Status == RouteListItemStatus.Overdue)
				{
					address.Order.ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
				}
			}
		}

		#endregion Адреса в МЛ
	}
}
