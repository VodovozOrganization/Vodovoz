using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Responses;
using DriverAPI.Library.Exceptions;
using DriverAPI.Library.V5.Converters;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using IDomainRouteListSpecialConditionsService = Vodovoz.Services.Logistics.IRouteListSpecialConditionsService;
using IDomainRouteListTransferService = Vodovoz.Services.Logistics.IRouteListTransferService;
using IDomainRouteListService = Vodovoz.Services.Logistics.IRouteListService;
using Vodovoz.Tools.CallTasks;

namespace DriverAPI.Library.V5.Services
{
	internal class RouteListService : IRouteListService
	{
		private readonly ILogger<RouteListService> _logger;
		private readonly IDomainRouteListSpecialConditionsService _domainRouteListSpecialConditionsService;
		private readonly IDomainRouteListTransferService _domainRouteListTransferService;
		private readonly IDomainRouteListService _domainRouteListService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly RouteListConverter _routeListConverter;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGenericRepository<Employee> _employeeGenericRepository;
		private readonly IGenericRepository<RouteListItem> _routeListAddressesRepository;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly PaymentTypeConverter _paymentTypeConverter;
		private readonly IFastPaymentService _fastPaymentService;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IUnitOfWork _unitOfWork;

		public RouteListService(ILogger<RouteListService> logger,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			RouteListConverter routeListConverter,
			IEmployeeRepository employeeRepository,
			IGenericRepository<Employee> employeeGenericRepository,
			IGenericRepository<RouteListItem> routeListAddressesRepository,
			IUnitOfWork unitOfWork,
			IDomainRouteListSpecialConditionsService domainRouteListSpecialConditionsService,
			IDomainRouteListTransferService domainRouteListTransferService,
			IDomainRouteListService domainRouteListService,
			IGenericRepository<Order> orderRepository,
			PaymentTypeConverter paymentTypeConverter,
			IFastPaymentService fastPaymentService,
			ICallTaskWorker callTaskWorker)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListConverter = routeListConverter ?? throw new ArgumentNullException(nameof(routeListConverter));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_employeeGenericRepository = employeeGenericRepository ?? throw new ArgumentNullException(nameof(employeeGenericRepository));
			_routeListAddressesRepository = routeListAddressesRepository ?? throw new ArgumentNullException(nameof(routeListAddressesRepository));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_domainRouteListSpecialConditionsService = domainRouteListSpecialConditionsService ?? throw new ArgumentNullException(nameof(domainRouteListSpecialConditionsService));
			_domainRouteListTransferService = domainRouteListTransferService ?? throw new ArgumentNullException(nameof(domainRouteListTransferService));
			_domainRouteListService = domainRouteListService ?? throw new ArgumentNullException(nameof(domainRouteListService));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentTypeConverter = paymentTypeConverter ?? throw new ArgumentNullException(nameof(paymentTypeConverter));
			_fastPaymentService = fastPaymentService ?? throw new ArgumentNullException(nameof(fastPaymentService));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
		}

		public RouteListDto GetRouteList(int routeListId)
		{
			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId)
				?? throw new DataNotFoundException(nameof(routeListId), $"Маршрутный лист {routeListId} не найден");

			IDictionary<int, string> spectiaConditionsToAccept = new Dictionary<int, string>();

			if(!routeList.SpecialConditionsAccepted)
			{
				spectiaConditionsToAccept = _domainRouteListSpecialConditionsService.GetSpecialConditionsDictionaryFor(_unitOfWork, routeListId);
			}

			return _routeListConverter.ConvertToAPIRouteList(routeList, _routeListRepository.GetDeliveryItemsToReturn(_unitOfWork, routeListId), spectiaConditionsToAccept);
		}

		public IEnumerable<RouteListDto> GetRouteLists(int[] routeListsIds)
		{
			var vodovozRouteLists = _routeListRepository.GetRouteListsByIds(_unitOfWork, routeListsIds);
			var routeLists = new List<RouteListDto>();

			foreach(var routeList in vodovozRouteLists)
			{
				try
				{
					IDictionary<int, string> spectiaConditionsToAccept = new Dictionary<int, string>();

					if(!routeList.SpecialConditionsAccepted)
					{
						spectiaConditionsToAccept = _domainRouteListSpecialConditionsService.GetSpecialConditionsDictionaryFor(_unitOfWork, routeList.Id);
					}

					routeLists.Add(_routeListConverter.ConvertToAPIRouteList(routeList, _routeListRepository.GetDeliveryItemsToReturn(_unitOfWork, routeList.Id), spectiaConditionsToAccept));
				}
				catch(ConverterException e)
				{
					_logger.LogWarning(e, "Ошибка конвертации маршрутного листа {RouteListId}", routeList.Id);
				}
			}

			return routeLists;
		}

		public Result<IEnumerable<int>> GetRouteListsIdsForDriverByAndroidLogin(string login)
		{
			var driver = _employeeRepository.GetEmployeeByAndroidLogin(_unitOfWork, login);

			if(driver is null)
			{
				return Result.Failure<IEnumerable<int>>(Vodovoz.Errors.Employees.DriverErrors.NotFound);
			}

			return Result.Success(_routeListRepository.GetDriverRouteListsIds(
				_unitOfWork,
				driver,
				RouteListStatus.EnRoute));
		}

		public Result RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime, int driverId)
		{
			var routeListAddress = _routeListItemRepository.GetRouteListItemById(_unitOfWork, routeListAddressId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ {RouteListItemId} не нейден", routeListAddressId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFound);
			}

			if(routeListAddress.Order is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.NotFound);
			}

			var deliveryPoint = routeListAddress.Order.DeliveryPoint;

			if(deliveryPoint is null)
			{
				return Result.Failure(Vodovoz.Errors.Clients.DeliveryPointErrors.NotFound);
			}

			if(routeListAddress.RouteList.Driver.Id != driverId)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки {RouteListAddressId} МЛ водителя {DriverId}, сотрудником {EmployeeId}",
					routeListAddressId,
					routeListAddress.RouteList.Driver.Id,
					driverId);

				return Result.Failure(Errors.Security.Authorization.RouteListAccessDenied);
			}

			if(routeListAddress.RouteList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки в МЛ {RouteListId} в статусе {RouteListStatus}" +
					" адреса {RouteListAddressId} в статусе {RouteListAddressStatus} водителем {DriverId}",
					routeListAddress.RouteList.Id,
					routeListAddress.RouteList.Status,
					routeListAddressId,
					routeListAddress.Status,
					driverId);

				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Попытка записи координаты точки доставки в МЛ {RouteListId} в статусе {RouteListStatus}" +
					" адреса {RouteListAddressId} в статусе {RouteListAddressStatus} водителем {DriverId}",
					routeListAddress.RouteList.Id,
					routeListAddress.RouteList.Status,
					routeListAddressId,
					routeListAddress.Status,
					driverId);

				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotEnRouteState);
			}

			var coordinate = new DeliveryPointEstimatedCoordinate()
			{
				DeliveryPointId = deliveryPoint.Id,
				Latitude = latitude,
				Longitude = longitude,
				RegistrationTime = actionTime
			};

			_unitOfWork.Save(coordinate);
			_unitOfWork.Commit();

			return Result.Success();
		}

		public string GetActualDriverPushNotificationsTokenByOrderId(int orderId)
		{
			return _employeeRepository.GetEmployeePushTokenByOrderId(_unitOfWork, orderId);
		}

		public Result RollbackRouteListAddressStatusEnRoute(int routeListAddressId, int driverId)
		{
			if(routeListAddressId <= 0)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFound);
			}

			var routeListAddress = _routeListItemRepository.GetRouteListItemById(_unitOfWork, routeListAddressId);

			if(routeListAddress is null)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFound);
			}

			if(!IsRouteListBelongToDriver(routeListAddress.RouteList.Id, driverId))
			{
				_logger.LogWarning("Попытка вернуть в путь адрес МЛ {RouteListAddressId} сотрудником {EmployeeId}, водитель МЛ: {DriverId}",
					routeListAddressId,
					driverId,
					routeListAddress.RouteList.Driver?.Id);

				return Result.Failure(Errors.Security.Authorization.RouteListAccessDenied);
			}

			if(routeListAddress.RouteList.Status != RouteListStatus.EnRoute)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.Completed)
			{
				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotCompletedState);
			}
			
			_domainRouteListService.ChangeAddressStatus(_unitOfWork, routeListAddress.RouteList, routeListAddress.Id, RouteListItemStatus.EnRoute, _callTaskWorker);

			_unitOfWork.Save(routeListAddress.RouteList);
			_unitOfWork.Save(routeListAddress);
			_unitOfWork.Commit();

			return Result.Success();
		}

		public bool IsRouteListBelongToDriver(int routeListId, int driverId)
		{
			var routeList = _routeListRepository.GetRouteListById(_unitOfWork, routeListId);

			return routeList?.Driver?.Id == driverId;
		}

		public Result<RouteListAddressIncomingTransferDto> GetIncomingTransferInfo(int routeListAddressId)
		{
			var address = _routeListAddressesRepository
				.Get(
					_unitOfWork,
					address => address.Id == routeListAddressId)
				.FirstOrDefault();

			var sourceResult = _domainRouteListTransferService.FindTransferSource(_unitOfWork, address);

			if(sourceResult.IsFailure)
			{
				return Result.Failure<RouteListAddressIncomingTransferDto>(sourceResult.Errors);
			}

			var transferItemsResult = GetTransferItems(address.Order.Id);

			if(transferItemsResult.IsFailure)
			{
				return Result.Failure<RouteListAddressIncomingTransferDto>(transferItemsResult.Errors);
			}

			var paid = _fastPaymentService
				.GetOrderFastPaymentStatus(address.Order.Id, address.Order.OnlinePaymentNumber) == FastPaymentStatus.Performed;

			return new RouteListAddressIncomingTransferDto
			{
				RouteListAddressTransferInfo = new RouteListAddressIncomingTransferInfo
				{
					RouteListAddressId = address.Id,
					OrderId = address.Order.Id,
					TransferringDriverId = sourceResult.Value.RouteList.Driver.Id,
					TransferringDriverTitle = sourceResult.Value.RouteList.Driver.ShortName,
					RecievingDriverId = address.RouteList.Driver.Id,
					RecievingDriverTitle = address.RouteList.Driver.ShortName,
					AcceptanceStatus = address.RecievedTransferAt == null ? AcceptanceStatus.NotAccepted : AcceptanceStatus.Accepted,
					TransferStatus = address.RecievedTransferAt == null ? TransferStatus.NotTransfered : TransferStatus.Transfered
				},
				PaymentType = _paymentTypeConverter.ConvertToAPIPaymentType(address.Order.PaymentType, paid, address.Order.PaymentByTerminalSource),
				TransferItems = transferItemsResult.Value
			};
		}

		public Result<RouteListAddressOutgoingTransferDto> GetOutgoingTransferInfo(int routeListAddressId)
		{
			var address = _routeListAddressesRepository.Get(
				_unitOfWork,
				address => address.Id == routeListAddressId
				&& (address.Order.OrderStatus == OrderStatus.OnTheWay
					|| address.Order.OrderStatus == OrderStatus.Shipped))
				.FirstOrDefault();

			var targetAddressResult = _domainRouteListTransferService.FindTransferTarget(_unitOfWork, address);

			if(targetAddressResult.IsFailure)
			{
				return Result.Failure<RouteListAddressOutgoingTransferDto>(targetAddressResult.Errors);
			}

			var transferItemsResult = GetTransferItems(address.Order.Id);

			if(transferItemsResult.IsFailure)
			{
				return Result.Failure<RouteListAddressOutgoingTransferDto>(transferItemsResult.Errors);
			}

			var paid = _fastPaymentService
				.GetOrderFastPaymentStatus(address.Order.Id, address.Order.OnlinePaymentNumber) == FastPaymentStatus.Performed;

			return new RouteListAddressOutgoingTransferDto
			{
				RouteListAddressTransferInfo = new RouteListAddressOutgoingTransferInfo
				{
					RouteListAddressId = address.Id,
					OrderId = address.Order.Id,
					TransferringDriverId = address.RouteList.Driver.Id,
					TransferringDriverTitle = address.RouteList.Driver.ShortName,
					RecievingDriverId = targetAddressResult.Value.RouteList.Driver.Id,
					RecievingDriverTitle = targetAddressResult.Value.RouteList.Driver.ShortName,
					AcceptanceStatus = targetAddressResult.Value.RecievedTransferAt == null ? AcceptanceStatus.NotAccepted : AcceptanceStatus.Accepted,
					TransferStatus = targetAddressResult.Value.RecievedTransferAt == null ? TransferStatus.NotTransfered : TransferStatus.Transfered
				},
				PaymentType = _paymentTypeConverter.ConvertToAPIPaymentType(address.Order.PaymentType, paid, address.Order.PaymentByTerminalSource),
				TransferItems = transferItemsResult.Value
			};
		}

		public Result<DriverTransfersInfoResponse> GetDriverDriverTransfers(int driverId)
		{
			var currentDriver = _employeeGenericRepository
				.Get(_unitOfWork, driver => driver.Id == driverId)
				.FirstOrDefault();

			if(currentDriver is null)
			{
				return Result.Failure<DriverTransfersInfoResponse>(Vodovoz.Errors.Employees.DriverErrors.NotFound);
			}

			return new DriverTransfersInfoResponse
			{
				IncomingTransfers = GetIncomingTransfers(currentDriver),
				OutgoingTransfers = GetOutgoingTransfers(currentDriver)
			};
		}

		private IEnumerable<RouteListAddressOutgoingTransferInfo> GetOutgoingTransfers(Employee driver)
		{
			var result = new List<RouteListAddressOutgoingTransferInfo>();

			var outgoingAddresses = _routeListAddressesRepository.Get(
				_unitOfWork,
				address => address.RouteList.Driver.Id == driver.Id
					&& address.Order.OrderStatus == OrderStatus.OnTheWay);

			foreach (var address in outgoingAddresses)
			{
				var targetAddressResult = _domainRouteListTransferService.FindTransferTarget(_unitOfWork, address);

				if(targetAddressResult.IsFailure)
				{
					continue;
				}

				result.Add(new RouteListAddressOutgoingTransferInfo
				{
					RouteListAddressId = address.Id,
					OrderId = address.Order.Id,
					TransferringDriverId = driver.Id,
					TransferringDriverTitle = driver.ShortName,
					RecievingDriverId = targetAddressResult.Value.RouteList.Driver.Id,
					RecievingDriverTitle = targetAddressResult.Value.RouteList.Driver.ShortName,
					AcceptanceStatus = targetAddressResult.Value.RecievedTransferAt is null ? AcceptanceStatus.NotAccepted : AcceptanceStatus.Accepted,
					TransferStatus = targetAddressResult.Value.RecievedTransferAt is null ? TransferStatus.NotTransfered : TransferStatus.Transfered
				});
			}

			return result;
		}

		private IEnumerable<RouteListAddressIncomingTransferInfo> GetIncomingTransfers(Employee driver)
		{
			var result = new List<RouteListAddressIncomingTransferInfo>();

			var statusesTransferInWork = new RouteListItemStatus[] { RouteListItemStatus.EnRoute, RouteListItemStatus.Transfered };

			var incomingAddresses = _routeListAddressesRepository.Get(
				_unitOfWork,
				address => address.RouteList.Driver.Id == driver.Id
					&& statusesTransferInWork.Contains(address.Status)
					&& address.AddressTransferType == AddressTransferType.FromHandToHand
					&& address.Order.OrderStatus == OrderStatus.OnTheWay);

			foreach(var address in incomingAddresses)
			{
				var sourceResult = _domainRouteListTransferService.FindTransferSource(_unitOfWork, address);

				if(sourceResult.IsSuccess)
				{
					result.Add(new RouteListAddressIncomingTransferInfo
					{
						RouteListAddressId = address.Id,
						OrderId = address.Order.Id,
						TransferringDriverId = sourceResult.Value.RouteList.Driver.Id,
						TransferringDriverTitle = sourceResult.Value.RouteList.Driver.ShortName,
						RecievingDriverId = driver.Id,
						RecievingDriverTitle = driver.ShortName,
						AcceptanceStatus = address.RecievedTransferAt == null ? AcceptanceStatus.NotAccepted : AcceptanceStatus.Accepted,
						TransferStatus = address.RecievedTransferAt == null ? TransferStatus.NotTransfered : TransferStatus.Transfered
					});
				}
				else
				{
					_logger.LogError("Не удалось найти источник переноса для адреса маршрутного листа {RouteListItemId}", address.Id);
				}
			}
		
			return result;
		}

		private Result<IEnumerable<TransferItemDto>> GetTransferItems(int orderId)
		{
			var order = _orderRepository.Get(_unitOfWork, o => o.Id == orderId).FirstOrDefault();

			if(order is null)
			{
				return Result.Failure<IEnumerable<TransferItemDto>>(Vodovoz.Errors.Orders.OrderErrors.NotFound);
			}

			var result = new List<TransferItemDto>();

			foreach(var item in order.OrderItems)
			{
				result.Add(new TransferItemDto
				{
					NomenclatureTitle = item.Nomenclature.Name,
					Amount = item.ActualCount ?? item.Count
				});
			}

			var equipmentsToClient = order.OrderEquipments.Where(oe => oe.Direction == Direction.Deliver);

			foreach(var item in equipmentsToClient)
			{
				result.Add(new TransferItemDto
				{
					NomenclatureTitle = item.Nomenclature.Name,
					Amount = item.ActualCount ?? item.Count
				});
			}

			return result;
		}
	}
}
