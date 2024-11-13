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
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class OrganizationCashTransferDocumentJournalViewModel : FilterableSingleEntityJournalViewModelBase<OrganizationCashTransferDocument, OrganizationCashTransferDocumentViewModel, OrganizationCashTransferDocumentJournalNode, OrganizationCashTransferDocumentFilterViewModel>
    {
        private readonly IEntityExtendedPermissionValidator _entityExtendedPermissionValidator;
        private readonly IEmployeeService _employeeService;
        
        public OrganizationCashTransferDocumentJournalViewModel(
	        OrganizationCashTransferDocumentFilterViewModel filterViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        ICommonServices commonServices,
	        IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
	        IEmployeeService employeeService)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            _entityExtendedPermissionValidator = entityExtendedPermissionValidator ?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            filterViewModel.HidenByDefault = true;

            TabName = "Журнал перемещения д/с для юр.лиц";
            UpdateOnChanges(typeof(OrganizationCashTransferDocument));
        }

        protected override Func<IUnitOfWork, IQueryOver<OrganizationCashTransferDocument>> ItemsSourceQueryFunction => (uow) =>
        {
            OrganizationCashTransferDocument organizationCashTransferDocumentAlias = null;
            OrganizationCashTransferDocumentJournalNode resultAlias = null;
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

            var itemsQuery = uow.Session.QueryOver(() => organizationCashTransferDocumentAlias);
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
                itemsQuery.Where(() => organizationCashTransferDocumentAlias.OrganizationFrom == FilterViewModel.OrganizationFrom);

            if (FilterViewModel.OrganizationTo != null)
                itemsQuery.Where(() => organizationCashTransferDocumentAlias.OrganizationTo == FilterViewModel.OrganizationTo);

            itemsQuery.Where(GetSearchCriterion(
                () => organizationCashTransferDocumentAlias.Id,
                () => organizationFromAlias.FullName,
                () => organizationToAlias.FullName)
            );

            itemsQuery
                .SelectList(list => list
                    .SelectGroup(() => organizationCashTransferDocumentAlias.Id).WithAlias(() => resultAlias.Id)
                    .Select(() => organizationCashTransferDocumentAlias.DocumentDate).WithAlias(() => resultAlias.DocumentDate)
                    .Select(authorProjection).WithAlias(() => resultAlias.Author)
                    .Select(() => organizationFromAlias.FullName).WithAlias(() => resultAlias.OrganizationFrom)
                    .Select(() => organizationToAlias.FullName).WithAlias(() => resultAlias.OrganizationTo)
                    .Select(() => organizationCashTransferDocumentAlias.TransferedSum).WithAlias(() => resultAlias.TransferedSum)
					.Select(() => organizationCashTransferDocumentAlias.Comment).WithAlias(() => resultAlias.Comment)
				)
                .OrderBy(x => x.DocumentDate).Asc
                .TransformUsing(Transformers.AliasToBean<OrganizationCashTransferDocumentJournalNode>());

            return itemsQuery;
        };

        protected override Func<OrganizationCashTransferDocumentViewModel> CreateDialogFunction =>
            () => new OrganizationCashTransferDocumentViewModel(
	            EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, _entityExtendedPermissionValidator, _employeeService);

        protected override Func<OrganizationCashTransferDocumentJournalNode, OrganizationCashTransferDocumentViewModel> OpenDialogFunction =>
            node => new OrganizationCashTransferDocumentViewModel(
	            EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, _entityExtendedPermissionValidator, _employeeService);
    }
}
