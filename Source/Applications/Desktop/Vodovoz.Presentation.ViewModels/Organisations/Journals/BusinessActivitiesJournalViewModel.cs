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
	public class BusinessActivitiesJournalViewModel :
		EntityJournalViewModelBase<BusinessActivity, BusinessActivityViewModel, BusinessActivityJournalNode>
	{
		private readonly BusinessActivitiesFilterViewModel _filterViewModel;

		public BusinessActivitiesJournalViewModel(
			BusinessActivitiesFilterViewModel filterViewModel,
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

		protected override IQueryOver<BusinessActivity> ItemsQuery(IUnitOfWork uow)
		{
			BusinessActivityJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<BusinessActivity>();
			
			if(_filterViewModel != null && !_filterViewModel.ShowArchived)
			{
				query.Where(ba => !ba.IsArchive);
			}
			
			query.SelectList(list => list
					.Select(ba => ba.Id).WithAlias(() => resultAlias.Id)
					.Select(ba => ba.Name).WithAlias(() => resultAlias.Name)
					.Select(ba => ba.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.TransformUsing(Transformers.AliasToBean<BusinessActivityJournalNode>());

			return query;
		}

		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
