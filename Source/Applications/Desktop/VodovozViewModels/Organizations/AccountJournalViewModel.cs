using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Organizations
{
	public class AccountJournalViewModel : EntityJournalViewModelBase<Account, AccountViewModel, AccountJournalNode>
	{
		private readonly AccountJournalFilterViewModel _accountJournalFilterViewModel;

		public AccountJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			AccountJournalFilterViewModel accountJournalFilterViewModel,
			Action<AccountJournalFilterViewModel> config = null) 
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(accountJournalFilterViewModel is null)
			{
				throw new ArgumentNullException(nameof(accountJournalFilterViewModel));
			}

			config?.Invoke(accountJournalFilterViewModel);

			_accountJournalFilterViewModel = accountJournalFilterViewModel;

			JournalFilter = _accountJournalFilterViewModel;

			VisibleCreateAction = false;
			VisibleDeleteAction = false;
			VisibleEditAction = false;
		}

		protected override IQueryOver<Account> ItemsQuery(IUnitOfWork uow)
		{
			Account accountAlias = null;
			Bank bankAlias = null;
			AccountJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => accountAlias)
				.JoinAlias(() => accountAlias.InBank, () => bankAlias);

			if(_accountJournalFilterViewModel.RestrictOrganizationId != null)
			{
				Account subqueryAccountAlias = null;

				var subqueryCriteria = QueryOver.Of<Organization>()
					.JoinAlias(x => x.Accounts, () => subqueryAccountAlias)
					.Where(x => x.Id == _accountJournalFilterViewModel.RestrictOrganizationId)
					.And(() => subqueryAccountAlias.Id == accountAlias.Id)
					.Select(x => x.Id)
					.DetachedCriteria;

				query.Where(Subqueries.Exists(subqueryCriteria));
			}

			if(_accountJournalFilterViewModel.RestrictCounterpartyId != null)
			{
				Account subqueryAccountAlias = null;

				var subqueryCriteria = QueryOver.Of<Counterparty>()
					.JoinAlias(x => x.Accounts, () => subqueryAccountAlias)
					.Where(x => x.Id == _accountJournalFilterViewModel.RestrictCounterpartyId)
					.And(() => subqueryAccountAlias.Id == accountAlias.Id)
					.Select(x => x.Id)
					.DetachedCriteria;

				query.Where(Subqueries.Exists(subqueryCriteria));
			}

			var result = query
				.Where(GetSearchCriterion(() => accountAlias.Number))
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Alias)
				.Select(x => x.IsDefault).WithAlias(() => resultAlias.IsDefault)
				.Select(() => bankAlias.Name).WithAlias(() => resultAlias.BankName)
				.Select(x => x.Number).WithAlias(() => resultAlias.AccountNumber))
				.TransformUsing(Transformers.AliasToBean<AccountJournalNode>())
				.OrderBy(x => x.Id).Asc;

			return result;
		}
	}
}
