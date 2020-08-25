using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.Views.Mango;
using System.Collections.Generic;
using Vodovoz.Dialogs;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Representations;
using Vodovoz.Reports;
using Vodovoz.Services.Permissions;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.EntityRepositories.Store;
using QS.Project.Journal;
using QSReport;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.Mango
{
	public class FullInternalCallViewModel : UowDialogViewModelBase
	{
		private ITdiCompatibilityNavigation tdiNavigation;

		public List<CounterpartyOrderViewModel> CounterpartyOrdersModels = new List<CounterpartyOrderViewModel>();

		private Counterparty currentCounterparty { get; set; }
		private Phone phone;

		public Phone Phone 
		{
			get => phone;
			private set { }
		}



		public FullInternalCallViewModel(IEnumerable<Counterparty> clients,Phone phone,INavigationManager navigation,ITdiCompatibilityNavigation tdinavigation, IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory, navigation)
		{
			this.NavigationManager = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.tdiNavigation= tdinavigation ?? throw new ArgumentNullException(nameof(navigation));
			Title = "Входящий звонок существующего контрагента";

			this.phone = phone ?? throw new ArgumentNullException(nameof(phone));
			Counterparty test_client = UoW.Session.Query<Counterparty>().Where(c => c.Id == 9).First();
			CounterpartyOrderViewModel test_model = new CounterpartyOrderViewModel(test_client, unitOfWorkFactory,navigation,tdiNavigation);
			CounterpartyOrdersModels.Add(test_model);

			//FIXME Удалить тестовую логику (вышеуказанную) и раскомментрировать рабочую логики (нижеуказанную)

			if(clients != null) {
				foreach(Counterparty client in clients) {
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, unitOfWorkFactory, navigation, tdinavigation);
					CounterpartyOrdersModels.Add(model);
				}
				currentCounterparty = CounterpartyOrdersModels.First().Client;
			} else
				throw new ArgumentNullException(nameof(clients));

		}


		public IDictionary<string, CounterpartyOrderView> GetCounterpartyViewModels() {
			return null;
		}
		#region Взаимодействие с Mangos

		//private IList<Counterparty> GetCounterpartiesByPhone(Phone phone)
		//{
		//	return IList<Counterparty>
		//}
		#endregion

		#region Действия View
		public void UpadateCurrentCounterparty(Counterparty counterparty)
		{
			currentCounterparty = counterparty;

		}


		public void AddComplainCommand()
		{
			var parameters = new Dictionary<string, object> {
				{"client", currentCounterparty},
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{"employeeSelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices)},
				{"counterpartySelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices)},
				{"phone", "не реализовано"}//FIXME
			};
			tdiNavigation.OpenTdiTabNamedArgs<CreateComplaintViewModel>(null, parameters);
		}
		public void NewOrderCommand()
		{

			tdiNavigation.OpenTdiTab<OrderDlg, Counterparty>(null, currentCounterparty);
		}


		public void BottleActCommand()
		{
			var parameters = new Vodovoz.Reports.RevisionBottlesAndDeposits();
			parameters.SetCounterparty(currentCounterparty);
			tdiNavigation.OpenTdiTab<ReportViewDlg, IParametersWidget>(null, parameters);
		}

		public void StockBalanceCommand()
		{
			NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(
		new WarehouseRepository()
	);
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, NomenclatureStockFilterViewModel>(null, filter);
			
		}

		public void CostAndDeliveryIntervalCommand()
		{
			//NavigationManager.OpenViewModel<>
		}

		public void NewClientCommand()
		{
			//NavigationManager.OpenViewModel<>
		}

		public void ExistingClientCommand()
		{
			//var page = tdiNavigation.OpenTdiTab<CounterpartyDlg>(null);
			//var tab = page.TdiTab as CounterpartyDlg;
			//tab.Entity.Phones.First().Number = "+7-000-000-00-00"; //FIXME
			//page.PageClosed += NewCounerpatry_PageClosed;
		}
		#endregion
	}
}
