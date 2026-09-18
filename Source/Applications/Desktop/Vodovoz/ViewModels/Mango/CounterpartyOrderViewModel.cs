using CustomerNotifications.Contracts;
using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using Notifications.Infrastructure;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Utilities.Extensions;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Application.Orders;
using Vodovoz.Core.Application.Orders.Services.OrderCancellation;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors.Logistics;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.Services.Orders;
using VodovozBusiness.NotificationSenders;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public class CounterpartyOrderViewModel : ViewModelBase, IDisposable
	{
		#region Свойства
		public Counterparty Client { get; private set; }
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITdiCompatibilityNavigation _tdiNavigation;
		private readonly IInteractiveService _interactiveService;
		private MangoManager MangoManager { get; set; }

		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IRouteListRepository _routedListRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListService _routeListService;
		private readonly IRouteListChangesNotificationSender _routeListChangesNotificationSender;

		private readonly OrderCancellationService _orderCancellationService;
		private readonly OrderCancellationPermitService _orderCancellationPermitService;
		private readonly IOutboxNotificationPublisher<CustomerNotificationDomainEvent> _customerNotificationPublisher;
		private IUnitOfWork UoW;
		
		private List<DeliveryPoint> _deliveryPoints = new List<DeliveryPoint>();
		private DeliveryPoint _deliveryPoint;
		private Order _selectedOrder;
		private UndeliveryViewModel _undeliveryViewModel;

		public List<Order> LatestOrder { get; private set; }
		public Order Order { get; set; }

		public Action RefreshOrders { get; private set; }

		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		public List<DeliveryPoint> DeliveryPoints
		{
			get => _deliveryPoints;
			set => SetField(ref _deliveryPoints, value);
		}

		public bool IsDeliveryPointChoiceRequired => 
			Client.Phones.All(p => p.DigitsNumber != MangoManager.CurrentCall.Phone.DigitsNumber)
			&& DeliveryPoints.Count > 1;

		#endregion

		#region Конструкторы

		public CounterpartyOrderViewModel(
			Counterparty client,
			IGtkTabsOpener gtkTabsOpener,
			IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation tdinavigation,
			IInteractiveService interactiveService,
			IRouteListRepository routedListRepository,
			MangoManager mangoManager,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
			IRouteListService routeListService,
			IRouteListChangesNotificationSender routeListChangesNotificationSender,
			OrderCancellationService orderCancellationService,
			OrderCancellationPermitService orderCancellationPermitService,
			IOutboxNotificationPublisher<CustomerNotificationDomainEvent> customerNotificationPublisher,
			int count = 5)
		{
			Client = client;
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_tdiNavigation = tdinavigation;
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_routedListRepository = routedListRepository ?? throw new ArgumentNullException(nameof(routedListRepository));
			MangoManager = mangoManager;
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_routeListChangesNotificationSender =
				routeListChangesNotificationSender ?? throw new ArgumentNullException(nameof(routeListChangesNotificationSender));
			_orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));
			_orderCancellationPermitService =
				orderCancellationPermitService ?? throw new ArgumentNullException(nameof(orderCancellationPermitService));
			_customerNotificationPublisher = customerNotificationPublisher ?? throw new ArgumentNullException(nameof(customerNotificationPublisher));
			UoW = _unitOfWorkFactory.CreateWithoutRoot();
			LatestOrder = _orderRepository.GetLatestOrdersForCounterparty(UoW, client, count).ToList();

			RefreshOrders = _RefreshOrders;
			NotifyConfiguration.Instance.BatchSubscribe(RefreshCounterparty)
				.IfEntity<Counterparty>()
				.AndWhere(c => c.Id == client.Id)
				.Or.IfEntity<DeliveryPoint>()
				.AndWhere(d => d.Counterparty?.Id == client.Id)
				.Or.IfEntity<Phone>()
				.AndWhere(p => p.Counterparty?.Id == client.Id || client.DeliveryPoints.Any(dp => dp.Id == p.DeliveryPoint?.Id));

			FillDeliveryPoints();
		}

		#endregion

		#region Функции

		#region privates

		private void RefreshCounterparty(EntityChangeEvent[] entities)
		{
			Client = UoW.GetById<Counterparty>(Client.Id);

			foreach(var entity in entities)
			{
				if(entity.EventType != TypeOfChangeEvent.Delete)
				{
					continue;
				}

				if(entity.Entity is DeliveryPoint deliveryPoint)
				{
					Client.DeliveryPoints?.Remove(Client.DeliveryPoints.SingleOrDefault(dp => dp.Id == deliveryPoint.Id));
				}

				if(entity.Entity is Phone phone)
				{
					if(phone.Counterparty != null)
					{
						Client.Phones?.Remove(Client.Phones.SingleOrDefault(p => p.Id == phone.Id));
					}

					if(phone.DeliveryPoint != null)
					{
						var phoneDeliveryPoint = Client.DeliveryPoints?.SingleOrDefault(dp => dp.Id == phone.DeliveryPoint.Id);
						phoneDeliveryPoint?.Phones?.Remove(phoneDeliveryPoint.Phones.SingleOrDefault(p => p.Id == phone.Id));
					}
				}
			}

			UoW.Session.Refresh(Client);

			FillDeliveryPoints();
		}

		private void FillDeliveryPoints()
		{
			if(Client.Phones.Any(p => p.DigitsNumber == MangoManager.CurrentCall.Phone.DigitsNumber))
			{
				DeliveryPoints?.Clear();
				DeliveryPoint = null;
			}
			else
			{
				DeliveryPoints = Client.DeliveryPoints?
					.Where(dp => dp.Phones.Any(p => p.DigitsNumber == MangoManager.CurrentCall.Phone.DigitsNumber))
					.ToList();


				if(DeliveryPoints?.Count == 1)
				{
					DeliveryPoint = DeliveryPoints.Single();
				}
				else
				{
					DeliveryPoint = null;
				}
			}

			OnPropertyChanged(nameof(IsDeliveryPointChoiceRequired));
		}

		private void _RefreshOrders()
		{
			LatestOrder = _orderRepository.GetLatestOrdersForCounterparty(UoW, Client, 5).ToList();
			OnPropertyChanged(nameof(LatestOrder));
		}
		#endregion
		
		public void OpenMoreInformationAboutCounterparty()
		{
			var page = _tdiNavigation.OpenTdiTab<CounterpartyDlg, int>(null, Client.Id, OpenPageOptions.IgnoreHash);
			var tab = page.TdiTab as CounterpartyDlg;
		}
		public void OpenMoreInformationAboutOrder(int id)
		{
			var page = _tdiNavigation.OpenTdiTab<OrderDlg, int>(null, id, OpenPageOptions.IgnoreHash);
		}

		public void RepeatOrder(Order order)
		{
			if(order.Id != 0)
			{
				_gtkTabsOpener.OpenOrderDlgForCopyOrderFromMangoByNavigator((DialogViewModelBase)null, order);
			}
		}

		public void OpenRoutedList(Order order)
		{
			if(order.OrderStatus == OrderStatus.NewOrder ||
				order.OrderStatus == OrderStatus.Accepted ||
				order.OrderStatus == OrderStatus.OnLoading
			) {
				_tdiNavigation.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());
			} else if(order.OrderStatus == OrderStatus.OnTheWay ||
			          order.OrderStatus == OrderStatus.InTravelList ||
			          order.OrderStatus == OrderStatus.Closed
			) {
				RouteList routeList = _routedListRepository.GetActualRouteListByOrder(UoW, order);
				if(routeList != null)
					_tdiNavigation.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(routeList.Id));
				
			} else if (order.OrderStatus == OrderStatus.Shipped) {
				RouteList routeList = _routedListRepository.GetActualRouteListByOrder(UoW, order);
				if(routeList != null)
					_tdiNavigation.OpenTdiTab<RouteListClosingDlg,RouteList>(null, routeList);
			}
		}

		public void OpenUndelivery(Order order)
		{
			var page = _tdiNavigation.OpenTdiTab<UndeliveredOrdersJournalViewModel>(null);
			var dlg = page.TdiTab as UndeliveredOrdersJournalViewModel;
			var filter = dlg.UndeliveredOrdersFilterViewModel;
			filter.HidenByDefault = true;
			filter.RestrictOldOrder = order;
			filter.RestrictOldOrderStartDate = order.DeliveryDate;
			filter.RestrictOldOrderEndDate = order.DeliveryDate;
		}

		/// <summary>
		/// Можно ли предлагать отмену заказа из окна звонка
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <returns>Признак возможности отмены заказа</returns>
		public bool CanCancelOrder(Order order) =>
			order != null
			&& (_orderRepository.GetStatusesForOrderCancelation().Contains(order.OrderStatus)
				|| (order.SelfDelivery && order.OrderStatus == OrderStatus.OnLoading));

		public void CancelOrder(Order order)
		{
			var permit = _orderCancellationPermitService.GetPermitWithOrderChecks(UoW, order);

			if(permit.Type != OrderCancellationPermitType.AllowCancelOrder)
			{
				return;
			}

			_undeliveryViewModel = _tdiNavigation.OpenViewModel<UndeliveryViewModel>(
				null,
				OpenPageOptions.None,
				vm =>
				{
					vm.Saved += OnUndeliveryViewModelSaved;
					vm.Initialize(UoW, order.Id, cancellationPermit: permit);
				}
			).ViewModel;
		}

		private void OnUndeliveryViewModelSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			var order = e.UndeliveredOrder.OldOrder;

			order.SetUndeliveredStatus(UoW, _routeListService, _nomenclatureSettings, _callTaskWorker,
				needCreateDeliveryFreeBalanceOperation: true);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, order);
			if(routeListItem != null)
			{
				routeListItem.StatusLastUpdate = DateTime.Now;
				routeListItem.SetOrderActualCountsToZeroOnCanceled();
				UoW.Save(routeListItem);
			}
			else
			{
				order.SetActualCountsToZeroOnCanceled();
			}

			UoW.Save(order);

			if(e.UndeliveredOrder.NewOrder != null)
			{
				var customerOrderRescheduledEvent = new CustomerNotificationDomainEvent(
					CustomerNotificationEventType.OrderRescheduled,
					e.UndeliveredOrder.OldOrder.OnlineOrder?.Source,
					e.UndeliveredOrder.OldOrder.OnlineOrder?.Id,
					e.UndeliveredOrder.OldOrder?.Id,
					e.UndeliveredOrder.NewOrder.Id,
					e.UndeliveredOrder.UndeliveryDetalization?.Name // Пока будут заполнять //e.UndeliveredOrder.UndeliveryDetalization?.CustomerNotificationText
					);

				_customerNotificationPublisher.TryPublish(UoW, customerOrderRescheduledEvent);
			}

			UoW.Commit();

			if(routeListItem != null)
			{
				NotifyDriverOfRouteListChanged(order.Id);
			}

			var allowCancellation = e.CancellationPermit.Type == OrderCancellationPermitType.AllowCancelOrder;
			var hasEdoTaskToCancellationId = e.CancellationPermit.EdoTaskToCancellationId != null;
			if(allowCancellation && hasEdoTaskToCancellationId)
			{
				_orderCancellationService.AutomaticCancelDocflow(
					UoW,
					$"Отмена заказа №{order.Id}",
					e.CancellationPermit.EdoTaskToCancellationId.Value
				);
			}

			_RefreshOrders();
		}

		private void NotifyDriverOfRouteListChanged(int orderId)
		{
			var notificationRequest = new NotificationRouteListChangesRequest
			{
				OrderId = orderId,
				PushNotificationDataEventType = PushNotificationDataEventType.RouteListContentChanged
			};

			var result = _routeListChangesNotificationSender.NotifyOfRouteListChanged(notificationRequest).GetAwaiter().GetResult();

			if(result.IsSuccess)
			{
				return;
			}

			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				string.Join(", ",
					result.Errors
						.Where(x => x.Code == RouteListErrors.RouteListItem.TransferTypeNotSet)
						.Select(x => x.Message))
				);
		}

		public void CreateComplaint(Order order)
		{
			if(order is null)
			{
				return;
			}

			var phoneNumber = "+7" + MangoManager.CurrentCall.Phone.Number;
			var viewModel = _tdiNavigation
				.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder, string>(null, EntityUoWBuilder.ForCreate(), phoneNumber)
				.ViewModel;
			
			viewModel.SetOrder(order.Id);
		}
		#endregion

		public Order SelectedOrder
		{
			get => _selectedOrder;
			set => SetField(ref _selectedOrder, value);
		}

		public void Dispose()
		{
			if(_undeliveryViewModel != null)
			{
				_undeliveryViewModel.Saved -= OnUndeliveryViewModelSaved;
			}

			NotifyConfiguration.Instance.UnsubscribeAll(this);
			RefreshOrders = null;
			UoW?.Dispose();
		}
	}
}
