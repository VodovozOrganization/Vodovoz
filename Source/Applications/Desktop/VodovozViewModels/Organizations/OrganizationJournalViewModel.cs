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
using NHibernate.SqlCommand;
using Vodovoz.Core.Domain.Edo;
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

			JournalFilter = _filterViewModel;

			_filterViewModel.OnFiltered += OnFilterFiltered;

			UpdateOnChanges(typeof(Organization));
		}

		protected override IQueryOver<Organization> ItemsQuery(IUnitOfWork uow)
		{
			Organization organizationAlias = null;
			TaxcomEdoSettings taxcomEdoSettingsAlias = null;
			OrganizationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => organizationAlias)
				.JoinEntityAlias(
					() => taxcomEdoSettingsAlias,
					() => organizationAlias.Id == taxcomEdoSettingsAlias.OrganizationId,
					JoinType.LeftOuterJoin);

			if(_filterViewModel.HasAvangardShopId)
			{
				query.Where(x => x.AvangardShopId != null);
			}

			if(_filterViewModel.HasCashBoxId)
			{
				query.Where(x => x.CashBoxId != null);
			}

			if(_filterViewModel.HasTaxcomEdoAccountId)
			{
				query.Where(x => taxcomEdoSettingsAlias.EdoAccount != null);
			}

			var hasAvangardShopIdProjection = Projections.Conditional(
				Restrictions.Where(() => organizationAlias.AvangardShopId == null),
				Projections.Constant(false),
				Projections.Constant(true));
			
			var hasTaxcomEdoAccountIdProjection = Projections.Conditional(
				Restrictions.Where(() => taxcomEdoSettingsAlias.EdoAccount == null),
				Projections.Constant(false),
				Projections.Constant(true));
			
			var hasCashBoxIdProjection = Projections.Conditional(
				Restrictions.Where(() => organizationAlias.CashBoxId == null),
				Projections.Constant(false),
				Projections.Constant(true));

			var invertedDisableDebtMailing = Projections.Conditional(
				Restrictions.Eq(Projections.Property<Organization>(x => x.DisableDebtMailing), true),
				Projections.Constant(false),
				Projections.Constant(true)
			);

			query.Where(
				GetSearchCriterion(
				() => organizationAlias.Name,
				() => organizationAlias.Id));

			var result = query.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(hasAvangardShopIdProjection).WithAlias(() => resultAlias.HasAvangardShopId)
				.Select(hasTaxcomEdoAccountIdProjection).WithAlias(() => resultAlias.HasTaxcomEdoAccountId)
				.Select(hasCashBoxIdProjection).WithAlias(() => resultAlias.HasCashBoxId)
				.Select(invertedDisableDebtMailing).WithAlias(() => resultAlias.SendDebtLetters)
				.Select(x => x.DebtMailingWithSignature).WithAlias(() => resultAlias.SendDebtLettersWithASignatureAndSeal)
				)
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
