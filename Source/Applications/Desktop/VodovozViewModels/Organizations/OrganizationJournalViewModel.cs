using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationJournalViewModel : EntityJournalViewModelBase<Organization, OrganizationViewModel, OrganizationJournalNode>
	{
		private readonly OrganizationJournalFilterViewModel _filterViewModel;

		public OrganizationJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			OrganizationJournalFilterViewModel organizationJournalFilterViewModel,
			Action<OrganizationJournalFilterViewModel> configure = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Организации";
			UseSlider = true;

			configure?.Invoke(organizationJournalFilterViewModel);

			_filterViewModel = organizationJournalFilterViewModel;

			_filterViewModel.OnFiltered += OnFilterFiltered;

			UpdateOnChanges(typeof(Organization));
		}

		protected override IQueryOver<Organization> ItemsQuery(IUnitOfWork uow)
		{
			Organization organizationAlias = null;
			OrganizationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => organizationAlias);

			if(_filterViewModel.IsAvangardShop)
			{
				query.Where(x => x.AvangardShopId != null);
			}

			query.Where(
				GetSearchCriterion(
				() => organizationAlias.Name,
				() => organizationAlias.Id));

			var result = query.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<OrganizationJournalNode>())
				.OrderBy(x => x.Id).Asc;

			return result;
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
