using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Validation;
using QS.ViewModels;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Mango
{
	public class CounterpartyOrderViewModel : ViewModelBase
	{
		#region Свойства
		public Counterparty Client { get; private set; }
		private ITdiCompatibilityNavigation tdiNavigation;
		private MangoManager MangoManager { get; set; }

		private readonly RouteListRepository routedListRepository;
		private IEmployeeRepository employeeRepository { get; set; } = EmployeeSingletonRepository.GetInstance();
		private IOrderRepository orderRepository { get; set; } = OrderSingletonRepository.GetInstance();
		private IRouteListItemRepository routeListItemRepository { get; set; } = new RouteListItemRepository();

		private IUnitOfWork UoW;

		public List<Order> LatestOrder { get; private set; }
		public Order Order { get; set; }

		public Action RefreshOrders { get; private set; }
		#endregion

		#region Конструкторы

		public CounterpartyOrderViewModel(Counterparty client,
			IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation tdinavigation,
			RouteListRepository routedListRepository,
			MangoManager mangoManager,
			int count = 5)
		: base()
		{
			this.Client = client;
			this.tdiNavigation = tdinavigation;
			this.routedListRepository = routedListRepository;
			this.MangoManager = mangoManager;
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			OrderSingletonRepository orderRepos = OrderSingletonRepository.GetInstance();
			LatestOrder = orderRepos.GetLatestOrdersForCounterparty(UoW, client, count).ToList();

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
			Client = UoW.GetById<Counterparty>(Client.Id);
		}
		private void _RefreshOrders()
		{
			LatestOrder = orderRepository.GetLatestOrdersForCounterparty(UoW, Client, 5).ToList();
			OnPropertyChanged(nameof(LatestOrder));
		}
		#endregion
		
		public void OpenMoreInformationAboutCounterparty()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg, int>(null, Client.Id, OpenPageOptions.IgnoreHash);
			var tab = page.TdiTab as CounterpartyDlg;
		}
		public void OpenMoreInformationAboutOrder(int id)
		{
			var page = tdiNavigation.OpenTdiTab<OrderDlg, int>(null, id, OpenPageOptions.IgnoreHash);
		}

		public void RepeatOrder(Order order)
		{
			if(order != null)
				tdiNavigation.OpenTdiTab<OrderDlg, Order, bool>(null, order, true, OpenPageOptions.IgnoreHash);
		}

		public void OpenRoutedList(Order order)
		{
			if(order.OrderStatus == OrderStatus.NewOrder ||
				order.OrderStatus == OrderStatus.Accepted ||
				order.OrderStatus == OrderStatus.OnLoading
			) {
				tdiNavigation.OpenTdiTab<RouteListCreateDlg>(null);
			} else if(order.OrderStatus == OrderStatus.OnTheWay ||
			          order.OrderStatus == OrderStatus.InTravelList ||
			          order.OrderStatus == OrderStatus.Closed
			) {
				RouteList routeList = routedListRepository.GetRouteListByOrder(UoW, order);
				if(routeList != null)
					tdiNavigation.OpenTdiTab<RouteListKeepingDlg, RouteList>(null, routeList);
				
			} else if (order.OrderStatus == OrderStatus.Shipped) {
				RouteList routeList = routedListRepository.GetRouteListByOrder(UoW, order);
				if(routeList != null)
					tdiNavigation.OpenTdiTab<RouteListClosingDlg,RouteList>(null, routeList);
			}
		}

		public void OpenUndelivery(Order order)
		{
			var page = tdiNavigation.OpenTdiTab<UndeliveriesView>(null);
			var dlg = page.TdiTab as UndeliveriesView;
			dlg.HideFilterAndControls();
			dlg.UndeliveredOrdersFilter.SetAndRefilterAtOnce(
				x => x.ResetFilter(),
				x => x.RestrictOldOrder = order,
				x => x.RestrictOldOrderStartDate = order.DeliveryDate,
				x => x.RestrictOldOrderEndDate = order.DeliveryDate
			);
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
				order.ChangeStatusAndCreateTasks(OrderStatus.Canceled, callTaskWorker);
				UoW.Save(order);
				UoW.Commit();
			}
		}

		public void CreateComplaint(Order order)
		{
			if (order != null)
			{
				var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());

				IEntityAutocompleteSelectorFactory employeeSelectorFactory =
					new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(
						ServicesConfig.CommonServices);

				IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
					new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
						CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

				IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
					new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig
							.CommonServices, new NomenclatureFilterViewModel(), counterpartySelectorFactory,
						nomenclatureRepository, UserSingletonRepository.GetInstance());

				ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();
				
				var parameters = new Dictionary<string, object> {
					{"order", order},
					{"uowBuilder", EntityUoWBuilder.ForCreate()},
					{ "unitOfWorkFactory",UnitOfWorkFactory.GetDefaultFactory },
					{"employeeSelectorFactory", employeeSelectorFactory},
					{"counterpartySelectorFactory", counterpartySelectorFactory},
					{"subdivisionService",subdivisionRepository},
					{"nomenclatureSelectorFactory" , nomenclatureSelectorFactory},
					{"nomenclatureRepository",nomenclatureRepository},
					{"phone", "+7" +this.MangoManager.CurrentCall.Phone.Number }
				};
				tdiNavigation.OpenTdiTabOnTdiNamedArgs<CreateComplaintViewModel>(null, parameters);
			}
		}
		#endregion
	}
}
