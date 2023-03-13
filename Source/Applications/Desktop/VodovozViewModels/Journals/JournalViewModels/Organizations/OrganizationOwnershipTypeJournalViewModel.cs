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
using Vodovoz.Factories;
using Vodovoz.EntityRepositories.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Organizations
{
	public class OrganizationOwnershipTypeJournalViewModel : FilterableSingleEntityJournalViewModelBase<OrganizationOwnershipType, OrganizationOwnershipTypeViewModel, OrganizationOwnershipTypeJournalNode, OrganizationOwnershipTypeJournalFilterViewModel>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IValidationContextFactory _validationContextFactory;
		private IOrganizationRepository _organizationRepository;

		public OrganizationOwnershipTypeJournalViewModel(
			OrganizationOwnershipTypeJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory,
			IOrganizationRepository organizationRepository) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));

			TabName = "Формы собственности контрагентов";
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
		   	_unitOfWorkFactory,
			_commonServices,
			_validationContextFactory,
			_organizationRepository
		);

		protected override Func<OrganizationOwnershipTypeJournalNode, OrganizationOwnershipTypeViewModel> OpenDialogFunction => node => new OrganizationOwnershipTypeViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
		   	_unitOfWorkFactory,
			_commonServices,
			_validationContextFactory,
			_organizationRepository
		);
	}
}
