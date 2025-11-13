using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using QS.DomainModel.UoW;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class CounterpartyEdoAccountsByOrganizationViewModel : UoWWidgetViewModelBase, IDisposable
	{
		private readonly Domain.Client.Counterparty _counterparty;
		private ILifetimeScope _scope;
		private ITdiTab _parentTab;

		public CounterpartyEdoAccountsByOrganizationViewModel(
			IUnitOfWork uow,
			ILifetimeScope scope,
			Domain.Client.Counterparty counterparty,
			IList<CounterpartyEdoAccount> counterpartyEdoOperators,
			ITdiTab parentTab)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_counterparty = counterparty ?? throw new ArgumentNullException(nameof(counterparty));
			_parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));
			
			Initialize(counterpartyEdoOperators);
		}

		public event Action<EdoAccountViewModel> EdoAccountViewModelAdded;
		public event Action<EdoAccountViewModel, int> EdoAccountViewModelRemoved;
		
		public EdoLightsMatrixViewModel EdoLightsMatrixViewModel { get; private set; }
		public IList<EdoAccountViewModel> EdoAccountsViewModels { get; set; }
		public int OrganizationId { get; private set; }
		
		public void SetOrganizationId(int organizationId)
		{
			OrganizationId = organizationId;
		}

		public void AddEdoAccount(CounterpartyEdoAccount edoAccount)
		{
			AddEdoAccountViewModel(edoAccount);
		}

		public void RefreshEdoLightsMatrixViewModel()
		{
			var currentDefaultEdoAccount = EdoAccountsViewModels
				.Select(x => x.Entity)
				.FirstOrDefault(x => x.IsDefault);

			if(currentDefaultEdoAccount is null)
			{
				return;
			}
			
			EdoLightsMatrixViewModel.RefreshLightsMatrix(currentDefaultEdoAccount);
		}

		private void Initialize(IList<CounterpartyEdoAccount> counterpartyEdoAccounts)
		{
			EdoLightsMatrixViewModel = _scope.Resolve<EdoLightsMatrixViewModel>();
			EdoAccountsViewModels = new List<EdoAccountViewModel>();
			
			foreach(var edoAccount in counterpartyEdoAccounts)
			{
				if(edoAccount.IsDefault)
				{
					EdoAccountOnPropertyChanged(edoAccount, new PropertyChangedEventArgs(nameof(edoAccount.IsDefault)));
				}
				
				AddEdoAccountViewModel(edoAccount);
			}

			if(EdoAccountsViewModels.Any() && EdoAccountsViewModels.All(x => !x.Entity.IsDefault))
			{
				EdoAccountsViewModels.First().Entity.IsDefault = true;
			}
		}

		private void EdoAccountOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(CounterpartyEdoAccount.IsDefault))
			{
				if(sender is CounterpartyEdoAccount edoAccount && edoAccount.IsDefault)
				{
					EdoLightsMatrixViewModel.RefreshLightsMatrix(edoAccount);
				}
			}
		}

		private void AddEdoAccountViewModel(CounterpartyEdoAccount edoAccount)
		{
			var edoAccountViewModel = _scope.Resolve<EdoAccountViewModel>(
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(Domain.Client.Counterparty), _counterparty),
				new TypedParameter(typeof(CounterpartyEdoAccount), edoAccount),
				new TypedParameter(typeof(ITdiTab), _parentTab)
			);

			EdoAccountsViewModels.Add(edoAccountViewModel);
			edoAccountViewModel.RefreshEdoLightsMatrixAction += RefreshEdoLightsMatrixViewModel;
			edoAccountViewModel.RemovedEdoAccountAction += RemovedEdoAccountViewModel;
			edoAccount.PropertyChanged += EdoAccountOnPropertyChanged;
			EdoAccountViewModelAdded?.Invoke(edoAccountViewModel);
		}
		
		private void RemovedEdoAccountViewModel(CounterpartyEdoAccount edoAccount)
		{
			var edoAccountViewModel = EdoAccountsViewModels.FirstOrDefault(x => x.Entity == edoAccount);
			
			if(edoAccountViewModel is null)
			{
				return;
			}
			
			EdoAccountViewModelUnsubscribeAll(edoAccountViewModel);
			edoAccount.PropertyChanged -= EdoAccountOnPropertyChanged;
			
			var edoAccountIndex = EdoAccountsViewModels.IndexOf(edoAccountViewModel);
			EdoAccountsViewModels.Remove(edoAccountViewModel);
			EdoAccountViewModelRemoved?.Invoke(edoAccountViewModel, edoAccountIndex);
		}

		private void EdoAccountViewModelUnsubscribeAll(EdoAccountViewModel edoAccountViewModel)
		{
			edoAccountViewModel.RefreshEdoLightsMatrixAction -= RefreshEdoLightsMatrixViewModel;
			edoAccountViewModel.RemovedEdoAccountAction -= RemovedEdoAccountViewModel;
		}

		public void Dispose()
		{
			foreach(var edoAccountViewModel in EdoAccountsViewModels)
			{
				EdoAccountViewModelUnsubscribeAll(edoAccountViewModel);
			}
			
			foreach(var edoAccount in _counterparty.CounterpartyEdoAccounts)
			{
				edoAccount.PropertyChanged -= EdoAccountOnPropertyChanged;
			}

			_scope = null;
			_parentTab = null;
		}
	}
}
