using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	public class UnknowTalkViewModel : TalkViewModelBase, IDisposable
	{
		private readonly ITdiCompatibilityNavigation _tdiNavigation;
		private readonly IInteractiveQuestion _interactive;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IUnitOfWork _uow;
		private ILifetimeScope _lifetimeScope;
		private IPage<CounterpartyJournalViewModel> _counterpartyJournalPage;
		
		public UnknowTalkViewModel(
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigation, 
			IInteractiveQuestion interactive,
			MangoManager manager,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			INomenclatureRepository nomenclatureRepository) : base(navigation, manager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_tdiNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_uow = unitOfWorkFactory.CreateWithoutRoot();
		}

		#region Действия View

		public void SelectNewConterparty()
		{
			var page = _tdiNavigation.OpenTdiTab<CounterpartyDlg, Phone>(this, ActiveCall.Phone);
			var tab = page.TdiTab as CounterpartyDlg;
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		public void SelectExistConterparty()
		{
			_counterpartyJournalPage = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			_counterpartyJournalPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			_counterpartyJournalPage.ViewModel.OnEntitySelectedResult -= OnExistingCounterpartyPageClosed;
			_counterpartyJournalPage.ViewModel.OnEntitySelectedResult += OnExistingCounterpartyPageClosed;
		}

		void NewCounerpatry_PageClosed(object sender, PageClosedEventArgs e)
		{ 
			if(e.CloseSource == CloseSource.Save) {
				Counterparty client = ((sender as TdiTabPage).TdiTab as CounterpartyDlg).Counterparty;
				if(client != null) {
					this.Close(true, CloseSource.External);
					MangoManager.AddCounterpartyToCall(client.Id);
				} else
					throw new Exception("При сохранении контрагента произошла ошибка, попробуйте снова." + "\n Сообщение для поддержки : UnknowTalkViewModel.NewCounterparty_PageClose()");
			}
		}

		void OnExistingCounterpartyPageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var counterpartyNode = e.SelectedNodes.First() as CounterpartyJournalNode;
			Counterparty client = _uow.GetById<Counterparty>(counterpartyNode.Id);
			if(_interactive.Question($"Добавить телефон к контрагенту {client.Name} ?", "Телефон контрагента")) {
				if(!client.Phones.Any(phone => phone.DigitsNumber == ActiveCall.Phone.DigitsNumber)) 
				{
					var phone = ActiveCall.Phone;
					phone.Counterparty = client;
					_uow.Save(phone);
					_uow.Commit();
				}
			}
			this.Close(true, CloseSource.External);
			MangoManager.AddCounterpartyToCall(client.Id);
		}

		public void CreateComplaintCommand()
		{
			var employeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			var counterpartySelectorFactory = _counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);

			var parameters = new Dictionary<string, object> {
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{ "unitOfWorkFactory", UnitOfWorkFactory.GetDefaultFactory },
				//Autofac: IEmployeeService 
				{"employeeSelectorFactory", employeeSelectorFactory},
				{"counterpartySelectorFactory", counterpartySelectorFactory},
				//Autofac: ICommonServices
				//Autofac: IUserRepository
				{"phone", "+7" + ActiveCall.Phone.Number }
			};
			
			_tdiNavigation.OpenTdiTabOnTdiNamedArgs<CreateComplaintViewModel>(null, parameters);
		}

		public void StockBalanceCommand()
		{
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel>(null);
		}

		public void CostAndDeliveryIntervalCommand()
		{
			_tdiNavigation.OpenTdiTab<DeliveryPriceDlg>(null);
		}

		public void Dispose()
		{
			if(_counterpartyJournalPage?.ViewModel != null)
			{
				_counterpartyJournalPage.ViewModel.OnEntitySelectedResult -= OnExistingCounterpartyPageClosed;
			}

			_lifetimeScope = null;
			_uow?.Dispose();
		}


		#endregion
	}
}
