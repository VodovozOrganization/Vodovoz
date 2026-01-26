using Autofac;
using FluentNHibernate.Data;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Utilities.Extensions;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Application.Orders;
using Vodovoz.Application.Orders.Services.OrderCancellation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public class CounterpartyOrderViewModel : ViewModelBase, IDisposable
	{
		#region Свойства
		public Counterparty Client { get; private set; }
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITdiCompatibilityNavigation _tdiNavigation;
		private MangoManager MangoManager { get; set; }
		private readonly IOrderSettings _orderSettings;

		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IRouteListRepository _routedListRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IRouteListService _routeListService;

		private readonly OrderCancellationService _orderCancellationService;
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
			IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation tdinavigation,
			IRouteListRepository routedListRepository,
			MangoManager mangoManager,
			IOrderSettings orderSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker,
			IEmployeeRepository employeeRepository,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
			ICallTaskRepository callTaskRepository,
			IRouteListService routeListService,
			OrderCancellationService orderCancellationService,
			int count = 5)
		{
			Client = client;
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_tdiNavigation = tdinavigation;
			_routedListRepository = routedListRepository ?? throw new ArgumentNullException(nameof(routedListRepository));
			MangoManager = mangoManager;
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_callTaskRepository = callTaskRepository ?? throw new ArgumentNullException(nameof(callTaskRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));

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

		public void RepeatOrder(int orderId)
		{
			if(orderId != 0)
				_tdiNavigation.OpenTdiTab<OrderDlg, int, bool>(null, orderId, true, OpenPageOptions.IgnoreHash);
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

		public void CancelOrder(Order order)
		{
			var employeeSettings = ScopeProvider.Scope.Resolve<IEmployeeSettings>();
			CallTaskWorker callTaskWorker = new CallTaskWorker(
				_unitOfWorkFactory,
				CallTaskSingletonFactory.GetInstance(),
				_callTaskRepository,
				_orderRepository,
				_employeeRepository,
				employeeSettings,
				ServicesConfig.CommonServices.UserService,
				ErrorReporter.Instance);

			if(order.OrderStatus == OrderStatus.InTravelList)
			{

				var validationContext = new ValidationContext(order, null, new Dictionary<object, object> {
					{ "NewStatus", OrderStatus.Canceled },
				});
				validationContext.ServiceContainer.AddService(_orderSettings);
				validationContext.ServiceContainer.AddService(_deliveryRulesSettings);
				if(!ServicesConfig.ValidationService.Validate(order, validationContext))
				{
					return;
				}

				var permit = _orderCancellationService.CanCancelOrder(UoW, order);
				switch(permit.Type)
				{
					case OrderCancellationPermitType.AllowCancelDocflow:
						if(permit.EdoTaskToCancellationId == null)
						{
							throw new InvalidOperationException("Для аннулирования документооборота должен быть указан идентификатор ЭДО задачи.");
						}
						_orderCancellationService.CancelDocflowByUser(order, permit.EdoTaskToCancellationId.Value);
						return;
					case OrderCancellationPermitType.AllowCancelOrder:
						break;
					case OrderCancellationPermitType.Deny:
					default:
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
			else
			{
				order.ChangeStatusAndCreateTasks(OrderStatus.Canceled, callTaskWorker);
				UoW.Save(order);
				UoW.Commit();
			}
		}

		private void OnUndeliveryViewModelSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			SelectedOrder.SetUndeliveredStatus(UoW, _routeListService, _nomenclatureSettings, _callTaskWorker);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, SelectedOrder);
			if(routeListItem != null && routeListItem.Status != RouteListItemStatus.Canceled)
			{
				_routeListService.SetAddressStatusWithoutOrderChange(UoW,  routeListItem.RouteList, routeListItem, RouteListItemStatus.Canceled);
				routeListItem.StatusLastUpdate = DateTime.Now;
				routeListItem.SetOrderActualCountsToZeroOnCanceled();
				UoW.Save(routeListItem.RouteList);
				UoW.Save(routeListItem);
			}

			UoW.Commit();

			var allowCancellation = e.CancellationPermit.Type == OrderCancellationPermitType.AllowCancelOrder;
			var hasEdoTaskToCancellationId = e.CancellationPermit.EdoTaskToCancellationId != null;
			if(allowCancellation && hasEdoTaskToCancellationId)
			{
				_orderCancellationService.AutomaticCancelDocflow(UoW, SelectedOrder, e.CancellationPermit.EdoTaskToCancellationId.Value);
			}
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
