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
	public class FundsJournalViewModel :
		EntityJournalViewModelBase<Funds, FundsViewModel, FundsJournalNode>
	{
		private readonly FundsFilterViewModel _filterViewModel;

		public FundsJournalViewModel(
			FundsFilterViewModel filterViewModel,
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

		protected override IQueryOver<Funds> ItemsQuery(IUnitOfWork uow)
		{
			FundsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<Funds>();

			if(_filterViewModel != null && !_filterViewModel.ShowArchived)
			{
				query.Where(f => !f.IsArchive);
			}

			query.SelectList(list => list
				.Select(f => f.Id).WithAlias(() => resultAlias.Id)
				.Select(f => f.Name).WithAlias(() => resultAlias.Name)
				.Select(f => f.DefaultAccountFillType).WithAlias(() => resultAlias.DefaultAccountFillType)
				.Select(f => f.IsArchive).WithAlias(() => resultAlias.IsArchive)
			)
			.TransformUsing(Transformers.AliasToBean<FundsJournalNode>());

			return query;
		}
		
		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterFiltered;
			base.Dispose();
		}
	}
}
