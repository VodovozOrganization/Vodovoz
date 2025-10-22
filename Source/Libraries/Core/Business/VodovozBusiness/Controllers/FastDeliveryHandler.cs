using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Models;
using Vodovoz.NotificationSenders;
using Vodovoz.Settings.Database.Logistics;
using Vodovoz.Settings.Logistics;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;

namespace Vodovoz.Controllers
{
	public class FastDeliveryHandler : IFastDeliveryHandler
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IDriverApiSettings _driverApiSettings;
		private readonly IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;
		private readonly IFastDeliveryValidator _fastDeliveryValidator;
		private readonly IFastDeliveryOrderAddedNotificationSender _fastDeliveryOrderAddedNotificationSender;

		public FastDeliveryHandler(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryRepository deliveryRepository,
			IDriverApiSettings driverApiSettings,
			IRouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController,
			IFastDeliveryValidator fastDeliveryValidator,
			IFastDeliveryOrderAddedNotificationSender fastDeliveryOrderAddedNotificationSender)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_driverApiSettings = driverApiSettings ?? throw new ArgumentNullException(nameof(driverApiSettings));
			_routeListAddressKeepingDocumentController =
				routeListAddressKeepingDocumentController ?? throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentController));
			_fastDeliveryValidator = fastDeliveryValidator ?? throw new ArgumentNullException(nameof(fastDeliveryValidator));
			_fastDeliveryOrderAddedNotificationSender = fastDeliveryOrderAddedNotificationSender ?? throw new ArgumentNullException(nameof(fastDeliveryOrderAddedNotificationSender));
		}
		
		public RouteList RouteListToAddFastDeliveryOrder { get; private set; }
		public FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory { get; private set; }

		public Result CheckFastDelivery(IUnitOfWork uow, Order order)
		{
			RouteListToAddFastDeliveryOrder = null;
			FastDeliveryAvailabilityHistory = null;
			
			if(order.IsFastDelivery)
			{
				var fastDeliveryValidationResult = _fastDeliveryValidator.ValidateOrder(order);

				if(fastDeliveryValidationResult.IsFailure)
				{
					return Result.Failure(fastDeliveryValidationResult.Errors);
				}
				
				FastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDeliveryForOrder(
					uow,
					(double)order.DeliveryPoint.Latitude.Value,
					(double)order.DeliveryPoint.Longitude.Value,
					isGetClosestByRoute: true,
					order.GetAllGoodsToDeliver(),
					order.DeliveryPoint.District.TariffZone.Id,
					order
				);

				var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(_unitOfWorkFactory);
				fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(FastDeliveryAvailabilityHistory);

				RouteListToAddFastDeliveryOrder = FastDeliveryAvailabilityHistory.Items
					.FirstOrDefault(x => x.IsValidToFastDelivery)
					?.RouteList;

				if(RouteListToAddFastDeliveryOrder is null)
				{
					return Result.Failure(Errors.Orders.FastDeliveryErrors.RouteListForFastDeliveryIsMissing);
				}
			}
			
			return Result.Success();
		}

		public async Task<Result> CheckFastDeliveryAsync(
			IUnitOfWork uow, 
			Order order, 
			CancellationToken cancellationToken
		)
		{
			RouteListToAddFastDeliveryOrder = null;
			FastDeliveryAvailabilityHistory = null;

			if(order.IsFastDelivery)
			{
				var fastDeliveryValidationResult = _fastDeliveryValidator.ValidateOrder(order);

				if(fastDeliveryValidationResult.IsFailure)
				{
					return Result.Failure(fastDeliveryValidationResult.Errors);
				}

				FastDeliveryAvailabilityHistory = await _deliveryRepository.GetRouteListsForFastDeliveryAsync(
					uow,
					(double)order.DeliveryPoint.Latitude.Value,
					(double)order.DeliveryPoint.Longitude.Value,
					isGetClosestByRoute: true,
					order.GetAllGoodsToDeliver(),
					order.DeliveryPoint.District.TariffZone.Id,
					cancellationToken
				);

				var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(_unitOfWorkFactory);
				await fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistoryAsync(
					FastDeliveryAvailabilityHistory,
					cancellationToken
				);

				RouteListToAddFastDeliveryOrder = FastDeliveryAvailabilityHistory.Items
					.FirstOrDefault(x => x.IsValidToFastDelivery)
					?.RouteList;

				if(RouteListToAddFastDeliveryOrder is null)
				{
					return Result.Failure(Errors.Orders.FastDeliveryErrors.RouteListForFastDeliveryIsMissing);
				}
			}

			return Result.Success();
		}

		public void TryAddOrderToRouteListAndNotifyDriver(IUnitOfWork uow, Order order, ICallTaskWorker callTaskWorker)
		{
			RouteListItem fastDeliveryAddress = null;

			if(RouteListToAddFastDeliveryOrder != null)
			{
				uow.Session.Refresh(RouteListToAddFastDeliveryOrder);
				fastDeliveryAddress = RouteListToAddFastDeliveryOrder.AddAddressFromOrder(order);
				
				order.ChangeStatusAndCreateTasks(OrderStatus.OnTheWay, callTaskWorker);
				order.UpdateDocuments();
			}

			if(fastDeliveryAddress != null)
			{
				uow.Session.Save(fastDeliveryAddress);

				_routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocument(
					uow, fastDeliveryAddress, DeliveryFreeBalanceType.Decrease);
			}
			
			NotifyDriverOfFastDeliveryOrderAdded(order.Id);
		}

		public void NotifyDriverOfFastDeliveryOrderAdded(int orderId)
		{
			if(RouteListToAddFastDeliveryOrder != null && DriverApiSettings.NotificationsEnabled)
			{
				_fastDeliveryOrderAddedNotificationSender.NotifyOfFastDeliveryOrderAdded(orderId);
			}
		}
	}
}
