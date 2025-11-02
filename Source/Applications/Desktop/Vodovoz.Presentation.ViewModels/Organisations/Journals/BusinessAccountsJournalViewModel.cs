using System;
using NHibernate;
using NHibernate.Criterion;
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
			_filterViewModel.JournalTab = this;
			JournalFilter = _filterViewModel;
			_filterViewModel.OnFiltered += OnFilterFiltered;

			_filterViewModel.IsShow = true;
			SearchEnabled = false;
		}

		protected override IQueryOver<BusinessAccount> ItemsQuery(IUnitOfWork uow)
		{
			BusinessAccount businessAccountAlias = null;
			BusinessActivity businessActivityAlias = null;
			Funds fundsAlias = null;
			BusinessAccountJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => businessAccountAlias)
				.JoinAlias(ba => ba.BusinessActivity, () => businessActivityAlias)
				.JoinAlias(ba => ba.Funds, () => fundsAlias);

			if(_filterViewModel != null && !_filterViewModel.ShowArchived)
			{
				query.Where(ba => !ba.IsArchive);
			}

			if(_filterViewModel != null && !string.IsNullOrWhiteSpace(_filterViewModel.Name))
			{
				query.Where(Restrictions.Like(Projections.Property(() => businessAccountAlias.Name),
					_filterViewModel.Name,
					MatchMode.Anywhere));
			}
			
			if(_filterViewModel != null && !string.IsNullOrWhiteSpace(_filterViewModel.Bank))
			{
				query.Where(Restrictions.Like(Projections.Property(() => businessAccountAlias.Bank),
					_filterViewModel.Bank,
					MatchMode.Anywhere));
			}

			if(_filterViewModel?.AccountFillType != null)
			{
				query.Where(ba => ba.AccountFillType == _filterViewModel.AccountFillType);
			}

			if(_filterViewModel != null && !string.IsNullOrWhiteSpace(_filterViewModel.Number))
			{
				query.Where(ba => ba.Number == _filterViewModel.Number);
			}
			
			if( _filterViewModel?.Funds != null)
			{
				query.Where(() => fundsAlias.Id == _filterViewModel.Funds.Id);
			}
			
			if( _filterViewModel?.BusinessActivity != null)
			{
				query.Where(() => businessActivityAlias.Id == _filterViewModel.BusinessActivity.Id);
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
			.TransformUsing(Transformers.AliasToBean<BusinessAccountJournalNode>())
			.OrderBy(ba => ba.Name)
			.Asc();

			return query;
		}

		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
