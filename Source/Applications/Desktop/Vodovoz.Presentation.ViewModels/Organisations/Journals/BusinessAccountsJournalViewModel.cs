using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	public class BusinessAccountsJournalViewModel :
		EntityJournalViewModelBase<BusinessAccount, BusinessAccountViewModel, BusinessAccountJournalNode>
	{
		private readonly BusinessAccountsFilterViewModel _filterViewModel;

		public BusinessAccountsJournalViewModel(
			BusinessAccountsFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			JournalFilter = _filterViewModel;
			_filterViewModel.OnFiltered += OnFilterFiltered;
		}

		protected override IQueryOver<BusinessAccount> ItemsQuery(IUnitOfWork uow)
		{
			BusinessActivity businessActivityAlias = null;
			Funds fundsAlias = null;
			BusinessAccountJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<BusinessAccount>()
				.JoinAlias(ba => ba.BusinessActivity, () => businessActivityAlias)
				.JoinAlias(ba => ba.Funds, () => fundsAlias);

			if(_filterViewModel != null && !_filterViewModel.ShowArchived)
			{
				query.Where(ba => !ba.IsArchive);
			}

			query.SelectList(list => list
				.Select(ba => ba.Id).WithAlias(() => resultAlias.Id)
				.Select(ba => ba.Name).WithAlias(() => resultAlias.Name)
				.Select(ba => ba.Bank).WithAlias(() => resultAlias.Bank)
				.Select(ba => ba.Number).WithAlias(() => resultAlias.Number)
				.Select(ba => ba.IsArchive).WithAlias(() => resultAlias.IsArchive)
				.Select(ba => ba.AccountFillType).WithAlias(() => resultAlias.AccountFillType)
				.Select(() => businessActivityAlias.Name).WithAlias(() => resultAlias.BusinessActivity)
				.Select(() => fundsAlias.Name).WithAlias(() => resultAlias.Funds)
			)
			.TransformUsing(Transformers.AliasToBean<BusinessAccountJournalNode>());

			return query;
		}

		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
