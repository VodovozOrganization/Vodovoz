using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Organizations
{
	public class OrganizationOwnershipTypeJournalViewModel : FilterableSingleEntityJournalViewModelBase<OrganizationOwnershipType, OrganizationOwnershipTypeViewModel, OrganizationOwnershipTypeJournalNode, OrganizationOwnershipTypeJournalFilterViewModel>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public OrganizationOwnershipTypeJournalViewModel(OrganizationOwnershipTypeJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Откуда клиент";
			SetOrder(x => x.Abbreviation);
			UpdateOnChanges(typeof(OrganizationOwnershipType));
		}

		protected override Func<IUnitOfWork, IQueryOver<OrganizationOwnershipType>> ItemsSourceQueryFunction => (uow) => {
			OrganizationOwnershipType organizationOwnershipTypeAlias = null;
			OrganizationOwnershipTypeJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => organizationOwnershipTypeAlias);
			if(!FilterViewModel.IsArchive)
				query.Where(() => !organizationOwnershipTypeAlias.IsArchive);

			query.Where(GetSearchCriterion(
				() => organizationOwnershipTypeAlias.Abbreviation,
				() => organizationOwnershipTypeAlias.FullName,
				() => organizationOwnershipTypeAlias.Id
			));

			var resultQuery = query
				.SelectList(list => list
				   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
				   .Select(x => x.Abbreviation).WithAlias(() => resultAlias.Abbreviation)
				   .Select(x => x.FullName).WithAlias(() => resultAlias.FullName)
				   .Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.OrderBy(x => x.Abbreviation).Asc
				.TransformUsing(Transformers.AliasToBean<OrganizationOwnershipTypeJournalNode>());

			return resultQuery;
		};

		protected override Func<OrganizationOwnershipTypeViewModel> CreateDialogFunction => () => new OrganizationOwnershipTypeViewModel(
			EntityUoWBuilder.ForCreate(),
		   	unitOfWorkFactory,
			commonServices
		);

		protected override Func<OrganizationOwnershipTypeJournalNode, OrganizationOwnershipTypeViewModel> OpenDialogFunction => node => new OrganizationOwnershipTypeViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			unitOfWorkFactory,
			commonServices
		);
	}
}
