using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class CounterpartyEdoAccountsViewModel : UoWWidgetViewModelBase, IDisposable
	{
		private ILifetimeScope _scope;
		private Domain.Client.Counterparty _counterparty;

		public CounterpartyEdoAccountsViewModel(
			IUnitOfWork uow,
			ILifetimeScope scope,
			Domain.Client.Counterparty counterparty,
			ITdiTab parentTab,
			ITdiCompatibilityNavigation navigationManager,
			IInteractiveService interactiveService
			)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			InteractiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_counterparty = counterparty ?? throw new ArgumentNullException(nameof(counterparty));
			ParentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			
			InitializeEdoAccounts();
			InitializeCommands();
		}

		public ITdiTab ParentTab { get; private set; }
		public ITdiCompatibilityNavigation NavigationManager { get; }
		public IInteractiveService InteractiveService { get; }

		public IObservableList<CounterpartyEdoAccount> CounterpartyEdoAccounts { get; private set; }
		public IDictionary<int, (string OrganizationName, CounterpartyEdoAccountsByOrganizationViewModel EdoAccountsViewModel)>
			EdoAccountsViewModelsByOrganizationId { get; private set; }
		public ICommand AddOrganizationCommand { get; private set; }
		public ICommand RemoveOrganizationCommand { get; private set; }
		public ICommand AddEdoAccountCommand { get; private set; }

		public void RefreshEdoLightsMatrices()
		{
			foreach(var edoAccountByOrganization in EdoAccountsViewModelsByOrganizationId)
			{
				edoAccountByOrganization.Value.EdoAccountsViewModel.RefreshEdoLightsMatrixViewModel();
			}
		}
		
		private void InitializeEdoAccounts()
		{
			CounterpartyEdoAccounts = _counterparty.CounterpartyEdoAccounts;
			EdoAccountsViewModelsByOrganizationId =
				new Dictionary<int, (string OrganizationName, CounterpartyEdoAccountsByOrganizationViewModel EdoAccountsViewModel)>();
			
			foreach(var edoAccountByOrganization in CounterpartyEdoAccounts.ToLookup(x => x.OrganizationId ?? 0))
			{
				AddEdoAccountsViewModelByOrganization(edoAccountByOrganization.Key, edoAccountByOrganization);
			}
		}

		private void AddEdoAccountsViewModelByOrganization(int organizationId, IEnumerable<CounterpartyEdoAccount> edoAccounts)
		{
			if(!EdoAccountsViewModelsByOrganizationId.TryGetValue(organizationId, out var orgNameWithEdoAccountsViewModel))
			{
				var edoAccountsViewModel = _scope.Resolve<CounterpartyEdoAccountsByOrganizationViewModel>(
					new TypedParameter(typeof(IUnitOfWork), UoW),
					new TypedParameter(typeof(Domain.Client.Counterparty), _counterparty),
					new TypedParameter(typeof(IList<CounterpartyEdoAccount>), edoAccounts),
					new TypedParameter(typeof(ITdiTab), ParentTab)
				);

				//TODO: возможно нужно брать из настройки
				if(organizationId == 0)
				{
					organizationId = 1;
				}
				
				var organization = UoW.GetById<Organization>(organizationId);
				orgNameWithEdoAccountsViewModel = (organization.Name, edoAccountsViewModel);
				orgNameWithEdoAccountsViewModel.EdoAccountsViewModel.SetOrganizationId(organizationId);
				EdoAccountsViewModelsByOrganizationId.Add(organizationId, orgNameWithEdoAccountsViewModel);
			}
		}

		private void InitializeCommands()
		{
			AddOrganizationCommand = new DelegateCommand<int>(AddOrganization);
			RemoveOrganizationCommand = new DelegateCommand(RemoveOrganization);
			
			AddEdoAccountCommand = new DelegateCommand<CounterpartyEdoAccountsByOrganizationViewModel>(AddEdoAccount);
		}

		private void AddOrganization(int organizationId)
		{
			var counterpartyEdoAccount = CounterpartyEdoAccount.Create(
				_counterparty,
				null,
				null,
				organizationId,
				true
			);

			CounterpartyEdoAccounts.Add(counterpartyEdoAccount);
			AddEdoAccountsViewModelByOrganization(
				organizationId,
				new List<CounterpartyEdoAccount>
				{
					counterpartyEdoAccount
				});
		}

		private void RemoveOrganization()
		{
			
		}

		private void AddEdoAccount(CounterpartyEdoAccountsByOrganizationViewModel edoAccountsByOrganizationViewModel)
		{
			var isDefault = !edoAccountsByOrganizationViewModel.EdoAccountsViewModels.Any();
			
			var newEdoAccount = CounterpartyEdoAccount.Create(
				_counterparty,
				null,
				null,
				edoAccountsByOrganizationViewModel.OrganizationId,
				isDefault
				);
			
			CounterpartyEdoAccounts.Add(newEdoAccount);
			edoAccountsByOrganizationViewModel.AddEdoAccount(newEdoAccount);
		}

		public void Dispose()
		{
			_scope = null;
			_counterparty = null;
			ParentTab = null;
		}
	}
}
