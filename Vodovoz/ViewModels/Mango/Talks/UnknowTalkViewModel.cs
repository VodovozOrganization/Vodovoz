using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalNodes;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public class UnknowTalkViewModel : TalkViewModelBase
	{
		private readonly ITdiCompatibilityNavigation tdiNavigation;
		private readonly IInteractiveQuestion interactive;
		private IUnitOfWork UoW;
		public UnknowTalkViewModel(IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigation, 
			IInteractiveQuestion interactive,
			MangoManager manager) : base(navigation, manager)
		{
			this.tdiNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			UoW = unitOfWorkFactory.CreateWithoutRoot();
		}

		#region Действия View

		public void SelectNewConterparty()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg,Phone>(this, ActiveCall.Phone);
			var tab = page.TdiTab as CounterpartyDlg;
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		public void SelectExistConterparty()
		{
			var page = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnEntitySelectedResult += ExistingCounterparty_PageClosed;
		}

		void NewCounerpatry_PageClosed(object sender, PageClosedEventArgs e)
		{ 
			if(e.CloseSource == CloseSource.Save) {
				Counterparty client = ((sender as TdiTabPage).TdiTab as CounterpartyDlg).Counterparty;
				if(client != null) {
					this.Close(false, CloseSource.External);
					MangoManager.AddCounterpartyToCall(client.Id);
				} else
					throw new Exception("При сохранении контрагента произошла ошибка, попробуйте снова." + "\n Сообщение для поддержки : UnknowTalkViewModel.NewCounterparty_PageClose()");
			}
		}

		void ExistingCounterparty_PageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var counterpartyNode = e.SelectedNodes.First() as CounterpartyJournalNode;
			Counterparty client = UoW.GetById<Counterparty>(counterpartyNode.Id);
			if(interactive.Question($"Добавить телефон к контрагенту {client.Name} ?", "Телефон контрагента")) {
				if(!client.Phones.Any(phone => phone.DigitsNumber == ActiveCall.Phone.DigitsNumber)) {
					client.Phones.Add(ActiveCall.Phone);
					UoW.Save<Counterparty>(client);
					UoW.Commit();
				}
			}
			this.Close(false, CloseSource.External);
			MangoManager.AddCounterpartyToCall(client.Id);
		}

		public void CreateComplaintCommand()
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
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{ "unitOfWorkFactory",UnitOfWorkFactory.GetDefaultFactory },
				//Autofac: IEmployeeService 
				{"employeeSelectorFactory", employeeSelectorFactory},
				{"counterpartySelectorFactory", counterpartySelectorFactory},
				{"subdivisionService",subdivisionRepository},
				//Autofac: ICommonServices
				{"nomenclatureSelectorFactory" , nomenclatureSelectorFactory},
				{"nomenclatureRepository",nomenclatureRepository},
				//Autofac: IUserRepository
				{"phone", "+7" + ActiveCall.Phone.Number }
			};
			tdiNavigation.OpenTdiTabOnTdiNamedArgs<CreateComplaintViewModel>(null, parameters);
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
			tdiNavigation.OpenTdiTab<DeliveryPriceDlg>(null);
		}

		#endregion

	}
}
