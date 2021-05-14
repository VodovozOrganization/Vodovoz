using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class OrganisationCashTransferDocumentJournalViewModel : FilterableSingleEntityJournalViewModelBase<OrganisationCashTransferDocument, OrganisationCashTransferDocumentViewModel, OrganisationCashTransferDocumentJournalNode, OrganisationCashTransferDocumentFilterViewModel>
    {
        private readonly IEntityExtendedPermissionValidator entityExtendedPermissionValidator;
        public OrganisationCashTransferDocumentJournalViewModel(OrganisationCashTransferDocumentFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IEntityExtendedPermissionValidator entityExtendedPermissionValidator)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал перемещения д/с для юр.лиц";
            UpdateOnChanges(typeof(OrganisationCashTransferDocument));
            this.entityExtendedPermissionValidator = entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
        }

        protected override Func<IUnitOfWork, IQueryOver<OrganisationCashTransferDocument>> ItemsSourceQueryFunction => (uow) =>
        {
            OrganisationCashTransferDocument organisationCashTransferDocumentAlias = null;
            OrganisationCashTransferDocumentJournalNode resultAlias = null;
            Employee authorAlias = null;
            Organization organizationFromAlias = null;
            Organization organizationToAlias = null;

            var authorProjection = Projections.SqlFunction(
                new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
                NHibernateUtil.String,
                Projections.Property(() => authorAlias.LastName),
                Projections.Property(() => authorAlias.Name),
                Projections.Property(() => authorAlias.Patronymic)
            );

            var itemsQuery = uow.Session.QueryOver(() => organisationCashTransferDocumentAlias);
            itemsQuery.Left.JoinAlias(x => x.OrganizationFrom, () => organizationFromAlias);
            itemsQuery.Left.JoinAlias(x => x.OrganizationTo, () => organizationToAlias);
            itemsQuery.Left.JoinAlias(x => x.Author, () => authorAlias);

            if (FilterViewModel.StartDate != null)
                itemsQuery.Where(o => o.DocumentDate >= FilterViewModel.StartDate);

            if (FilterViewModel.EndDate != null)
                itemsQuery.Where(o => o.DocumentDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));

            if (FilterViewModel.Author != null)
                itemsQuery.Where(o => o.Author.Id == FilterViewModel.Author.Id);

            if (FilterViewModel.OrganizationFrom != null)
                itemsQuery.Where(() => organisationCashTransferDocumentAlias.OrganizationFrom == FilterViewModel.OrganizationFrom);

            if (FilterViewModel.OrganizationTo != null)
                itemsQuery.Where(() => organisationCashTransferDocumentAlias.OrganizationTo == FilterViewModel.OrganizationTo);

            itemsQuery.Where(GetSearchCriterion(
                () => organizationFromAlias.FullName,
                () => organizationToAlias.FullName)
            );

            itemsQuery
                .SelectList(list => list
                    .SelectGroup(() => organisationCashTransferDocumentAlias.Id).WithAlias(() => resultAlias.Id)
                    .Select(() => organisationCashTransferDocumentAlias.DocumentDate).WithAlias(() => resultAlias.DocumentDate)
                    .Select(authorProjection).WithAlias(() => resultAlias.Author)
                    .Select(() => organizationFromAlias.FullName).WithAlias(() => resultAlias.OrganizationFrom)
                    .Select(() => organizationToAlias.FullName).WithAlias(() => resultAlias.OrganizationTo)
                    .Select(() => organisationCashTransferDocumentAlias.TransferedSum).WithAlias(() => resultAlias.TransferedSum)
                )
                .OrderBy(x => x.DocumentDate).Asc
                .TransformUsing(Transformers.AliasToBean<OrganisationCashTransferDocumentJournalNode>());

            return itemsQuery;
        };

        protected override Func<OrganisationCashTransferDocumentViewModel> CreateDialogFunction =>
            () => new OrganisationCashTransferDocumentViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, entityExtendedPermissionValidator);

        protected override Func<OrganisationCashTransferDocumentJournalNode, OrganisationCashTransferDocumentViewModel> OpenDialogFunction =>
            node => new OrganisationCashTransferDocumentViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, entityExtendedPermissionValidator);
    }
}
