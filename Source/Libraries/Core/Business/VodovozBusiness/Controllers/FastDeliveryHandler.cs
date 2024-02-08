using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Errors;
using Vodovoz.Models;
using Vodovoz.NotificationRecievers;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;

namespace Vodovoz.Controllers
{
	public class FastDeliveryHandler
	{
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IDriverApiParametersProvider _driverApiParametersProvider;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;
		private readonly IFastDeliveryValidator _fastDeliveryValidator;

		public FastDeliveryHandler(
			IDeliveryRepository deliveryRepository,
			IDriverApiParametersProvider driverApiParametersProvider,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IRouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController,
			IFastDeliveryValidator fastDeliveryValidator)
		{
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_driverApiParametersProvider =
				driverApiParametersProvider ?? throw new ArgumentNullException(nameof(driverApiParametersProvider));
			_deliveryRulesParametersProvider =
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_routeListAddressKeepingDocumentController =
				routeListAddressKeepingDocumentController ?? throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentController));
			_fastDeliveryValidator = fastDeliveryValidator ?? throw new ArgumentNullException(nameof(fastDeliveryValidator));
		}
		
		private DriverAPIHelper _driverApiHelper;

		public virtual DriverAPIHelper DriverApiHelper
		{
			get
			{
				if(_driverApiHelper == null)
				{
					var driverApiConfig = new DriverApiHelperConfiguration
					{
						ApiBase = _driverApiParametersProvider.ApiBase,
						NotifyOfSmsPaymentStatusChangedURI = _driverApiParametersProvider.NotifyOfSmsPaymentStatusChangedUri,
						NotifyOfFastDeliveryOrderAddedUri = _driverApiParametersProvider.NotifyOfFastDeliveryOrderAddedUri,
						NotifyOfWaitingTimeChangedUri = _driverApiParametersProvider.NotifyOfWaitingTimeChangedURI
					};
					_driverApiHelper = new DriverAPIHelper(new LoggerFactory().CreateLogger<DriverAPIHelper>(), driverApiConfig);
				}

				return _driverApiHelper;
			}
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
				
				FastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDelivery(
					uow,
					(double)order.DeliveryPoint.Latitude.Value,
					(double)order.DeliveryPoint.Longitude.Value,
					isGetClosestByRoute: true,
					_deliveryRulesParametersProvider,
					order.GetAllGoodsToDeliver(),
					order
				);

				var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(UnitOfWorkFactory.GetDefaultFactory);
				fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(FastDeliveryAvailabilityHistory);

				RouteListToAddFastDeliveryOrder = FastDeliveryAvailabilityHistory.Items
					.FirstOrDefault(x => x.IsValidToFastDelivery)
					?.RouteList;

				if(RouteListToAddFastDeliveryOrder == null)
				{
					return Result.Failure(Errors.Orders.Order.FastDelivery.RouteListForFastDeliveryIsMissing);
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
			if(RouteListToAddFastDeliveryOrder != null && DriverApiParametersProvider.NotificationsEnabled)
			{
				DriverApiHelper.NotifyOfFastDeliveryOrderAdded(orderId);
			}
		}
	}
}
