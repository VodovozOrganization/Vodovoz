using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Validation;
using QS.ViewModels;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.JournalViewers;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.ViewModels.Mango
{
	public class CounterpartyArguments
	{

	}
	public class CounterpartyOrderViewModel : ViewModelBase
	{
		#region Свойства

		private Counterparty client;
		public Counterparty Client {
			get { return client;}
			private set { client = value; }
		}
		private ITdiCompatibilityNavigation tdiNavigation;

		private readonly RouteListRepository routedListRepository;
		private IEmployeeRepository employeeRepository { get; set; } = EmployeeSingletonRepository.GetInstance();
		private IOrderRepository orderRepository { get; set; } = OrderSingletonRepository.GetInstance();
		private IRouteListItemRepository routeListItemRepository { get; set; } = new RouteListItemRepository();

		private IUnitOfWork UoW;

		public List<Order> LatestOrder {get;private set;}
		public Order Order { get; set; }

		public Action RefreshOrders { get; private set; }
		#endregion

		#region Конструкторы

		public CounterpartyOrderViewModel(Counterparty client,
			IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation tdinavigation,
			RouteListRepository routedListRepository,
			//IInteractiveMessage interactive,
			int count = 5) 
		: base()
		{
			this.client = client;
			this.tdiNavigation = tdinavigation;
			this.routedListRepository = routedListRepository;
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			OrderSingletonRepository orderRepos = OrderSingletonRepository.GetInstance();
			LatestOrder = orderRepos.GetLatestOrdersForCounterparty(UoW,client,count).ToList();

			RefreshOrders = _RefreshOrders;
			NotifyConfiguration.Instance.BatchSubscribe(_RefreshCounterparty)
				.IfEntity<Counterparty>()
				.AndWhere(c => c.Id == client.Id)
				.Or.IfEntity<DeliveryPoint>()
				.AndWhere(d => client.DeliveryPoints.Any(cd => cd.Id == d.Id));

		}
		#endregion

		#region Функции

		#region privates
		private void _RefreshCounterparty(EntityChangeEvent[] entity)
		{
			client = UoW.GetById<Counterparty>(client.Id);
		}
		private void _RefreshOrders()
		{
			LatestOrder = orderRepository.GetLatestOrdersForCounterparty(UoW, Client, 5).ToList();
			OnPropertyChanged(nameof(LatestOrder));
		}
		#endregion


		public void OpenMoreInformationAboutCounterparty()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg, int>(null, client.Id, OpenPageOptions.IgnoreHash);
			var tab = page.TdiTab as CounterpartyDlg;
		}
		public void OpenMoreInformationAboutOrder(int id)
		{
			var page = tdiNavigation.OpenTdiTab<OrderDlg,int>(null, id,OpenPageOptions.IgnoreHash);
		}

		public void RepeatOrder(Order order)
		{
			if(order != null)
				tdiNavigation.OpenTdiTab<OrderDlg, Order, bool>(null, order, true);
		}

		public void OpenRoutedList(Order order)
		{
			if(order.OrderStatus == OrderStatus.OnLoading) { 

			} else if(order.OrderStatus == OrderStatus.OnTheWay) {
				RouteList routeList = routedListRepository.GetRouteListByOrder(UoW, order);
				if(routeList != null)
					tdiNavigation.OpenTdiTab<RouteListKeepingDlg, RouteList>(null, routeList);
			} else if (order.OrderStatus == OrderStatus.UnloadingOnStock) {
			
			}

		}

		public void OpenUnderlivery(Order order)
		{
			UndeliveredOrdersFilter undeliveredOrdersFilter = new UndeliveredOrdersFilter();
			undeliveredOrdersFilter.SetAndRefilterAtOnce(
				x => x.ResetFilter(),
				x => x.RestrictOldOrder = order,
				x => x.RestrictOldOrderStartDate = order.DeliveryDate,
				x => x.RestrictOldOrderEndDate = order.DeliveryDate
			);
			IPage page = tdiNavigation.OpenTdiTab<UndeliveriesView, UndeliveredOrdersFilter>(null, undeliveredOrdersFilter);
		}

		public void CancelOrder(Order order)
		{
			CallTaskWorker callTaskWorker = new CallTaskWorker(
							CallTaskSingletonFactory.GetInstance(),
							new CallTaskRepository(),
							orderRepository,
							employeeRepository,
							new BaseParametersProvider(),
							ServicesConfig.CommonServices.UserService,
							SingletonErrorReporter.Instance);

			if(order.OrderStatus == OrderStatus.InTravelList) {

				var valid = new QSValidator<Order>(order,
				new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.Canceled },
				});
				if(valid.RunDlgIfNotValid(null))
					return;

				ITdiPage page = tdiNavigation.OpenTdiTab<UndeliveryOnOrderCloseDlg, Order, IUnitOfWork>(null, order, UoW);
				page.PageClosed += (sender, e) => {
					order.SetUndeliveredStatus(UoW, new BaseParametersProvider(), callTaskWorker);

					var routeListItem = routeListItemRepository.GetRouteListItemForOrder(UoW, order);
					if(routeListItem != null && routeListItem.Status != RouteListItemStatus.Canceled) {
						routeListItem.SetStatusWithoutOrderChange(RouteListItemStatus.Canceled);
						routeListItem.StatusLastUpdate = DateTime.Now;
						routeListItem.FillCountsOnCanceled();
						UoW.Save(routeListItem);
					}

					UoW.Commit();
				};
			} else {
				order.ChangeStatus(OrderStatus.Canceled, callTaskWorker);
				UoW.Save(order);
				UoW.Commit();
			}
		}

		public void CreateComplaint(Order order)
		{
		}
		#endregion


	}
}
