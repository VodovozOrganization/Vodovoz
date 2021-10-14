using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.Interactive;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class CashRequestJournalViewModel: FilterableSingleEntityJournalViewModelBase
        <
            CashRequest,
            CashRequestViewModel,
            CashRequestJournalNode,
            CashRequestJournalFilterViewModel
        >
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IFileChooserProvider fileChooserProvider;
        private readonly IEmployeeRepository employeeRepository;
        private readonly CashRepository cashRepository;
        private readonly ConsoleInteractiveService consoleInteractiveService;
        private readonly IEmployeeJournalFactory _employeeJournalFactory;
        private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
        
        public CashRequestJournalViewModel(
            CashRequestJournalFilterViewModel filterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IFileChooserProvider fileChooserProvider,
            IEmployeeRepository employeeRepository,
            CashRepository cashRepository,
            ConsoleInteractiveService consoleInteractiveService,
            IEmployeeJournalFactory employeeJournalFactory,
            ISubdivisionJournalFactory subdivisionJournalFactory
        ) : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            this.cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
            this.consoleInteractiveService = consoleInteractiveService ?? throw new ArgumentNullException(nameof(consoleInteractiveService));
            _employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            _subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));

            TabName = "Журнал заявок ДС";
            
            UpdateOnChanges(
                typeof(CashRequest),
                typeof(CashRequestSumItem),
                typeof(Subdivision),
                typeof(Employee)
            );
        }

        protected override Func<IUnitOfWork, IQueryOver<CashRequest>> ItemsSourceQueryFunction => (uow) =>
        {
            CashRequest cashRequestAlias = null;
            Employee authorAlias = null;
            CashRequestSumItem cashRequestSumItemAlias = null;
            Employee accountableEmployeeAlias = null;

            CashRequestJournalNode resultAlias = null;

            var result = uow.Session.QueryOver<CashRequest>(() => cashRequestAlias)
                .Left.JoinAlias(с => с.Sums, () => cashRequestSumItemAlias)
                .Left.JoinAlias(с => с.Author, () => authorAlias)
                .Left.JoinAlias(() => cashRequestSumItemAlias.AccountableEmployee, () => accountableEmployeeAlias);
            
            if(FilterViewModel != null) {
                if(FilterViewModel.StartDate.HasValue)
                    result.Where(() => cashRequestAlias.Date >= FilterViewModel.StartDate.Value.Date);
                if(FilterViewModel.EndDate.HasValue)
                    result.Where(() => cashRequestAlias.Date < FilterViewModel.EndDate.Value.Date.AddDays(1));
                if(FilterViewModel.Author != null)
                    result.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
                if (FilterViewModel.AccountableEmployee != null)
                    result.Where(() => accountableEmployeeAlias.Id == FilterViewModel.AccountableEmployee.Id);
                if (FilterViewModel.State != null )
                {
                    result.Where(() => cashRequestAlias.State == FilterViewModel.State);
                }
            }

            var userId = commonServices.UserService.CurrentUserId;
            var currentEmployee = employeeRepository.GetEmployeesForUser(uow, userId).First();
            var currentEmployeeId = currentEmployee.Id;

            if (!commonServices.UserService.GetCurrentUser(UoW).IsAdmin)
            {
                if (!commonServices.PermissionService
                        .ValidateUserPresetPermission("role_financier_cash_request", userId)
                    && !commonServices.PermissionService
                        .ValidateUserPresetPermission("role_coordinator_cash_request", userId)
                    && !commonServices.PermissionService
                        .ValidateUserPresetPermission("role_сashier", userId)
                    )
                {
                    if (commonServices.CurrentPermissionService.ValidatePresetPermission("can_see_current_subdivision_cash_requests"))
                    {
                        result.Where(() => authorAlias.Subdivision.Id == currentEmployee.Subdivision.Id);
                    }
                    else
                    {
                        result.Where(() => cashRequestAlias.Author.Id == currentEmployeeId);
                    }
                }

            }

            var authorProjection = Projections.SqlFunction(
                new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
                NHibernateUtil.String,
                Projections.Property(() => authorAlias.LastName),
                Projections.Property(() => authorAlias.Name),
                Projections.Property(() => authorAlias.Patronymic)
            );
            
            var accauntableProjection = Projections.SqlFunction(
                new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
                NHibernateUtil.String,
                Projections.Property(() => accountableEmployeeAlias.LastName),
                Projections.Property(() => accountableEmployeeAlias.Name),
                Projections.Property(() => accountableEmployeeAlias.Patronymic)
            );

            var cashReuestSumSubquery = QueryOver.Of(() => cashRequestSumItemAlias)
                .Where(() => cashRequestSumItemAlias.CashRequest.Id == cashRequestAlias.Id)
                .Where(Restrictions.IsNotNull(Projections.Property<CashRequestSumItem>(o => o.CashRequest)))
                .Select(Projections.Sum(() => cashRequestSumItemAlias.Sum));
            
            result.Where(GetSearchCriterion(
                () => authorAlias.Id,
                () => authorProjection,
                () => accauntableProjection,
                () => accountableEmployeeAlias.Id,
                () => cashRequestAlias.Basis
            ));
            
            result.SelectList(list => list
                    .SelectGroup(c => c.Id)
                    .Select(c => c.Id).WithAlias(() => resultAlias.Id)
                    .Select(c => c.Date).WithAlias(() => resultAlias.Date)
                    .Select(c => c.State).WithAlias(() => resultAlias.State)
                    .Select(c => c.DocumentType).WithAlias(() => resultAlias.DocumentType) 
                    .Select(authorProjection).WithAlias(() => resultAlias.Author)
                    .Select(accauntableProjection).WithAlias(() => resultAlias.AccountablePerson)
                    .SelectSubQuery(cashReuestSumSubquery).WithAlias(() => resultAlias.Sum)
                    .Select(c => c.Basis).WithAlias(() => resultAlias.Basis)
                ).TransformUsing(Transformers.AliasToBean<CashRequestJournalNode>())
                .OrderBy(x => x.Date).Desc();
            return result;
        };

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultSelectAction();
            CreateDefaultAddActions();
            CreateDefaultEditAction();
            CreateDeleteAction();
        }
        
        void CreateDeleteAction()
        {
            var deleteAction = new JournalAction("Удалить",
                (selected) => {
                    
                    var selectedNodes = selected.OfType<CashRequestJournalNode>();

                    if(selectedNodes == null || selectedNodes.Count() != 1) {
                        return false;
                    }

                    CashRequestJournalNode selectedNode = selectedNodes.First();

                    if (selectedNode.State != CashRequest.States.New){
                        return false;
                    }
                    
                    
                    if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
                        return false;
                    }
                    
                    var config = EntityConfigs[selectedNode.EntityType];
                    return config.PermissionResult.CanDelete;
                },
                (selected) => true,
                (selected) => {
                    var selectedNodes = selected.OfType<CashRequestJournalNode>();
                    if(selectedNodes == null || selectedNodes.Count() != 1) {
                        return;
                    }
                    CashRequestJournalNode selectedNode = selectedNodes.First();
                    if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
                        return;
                    }
                    var config = EntityConfigs[selectedNode.EntityType];
                    if(config.PermissionResult.CanDelete) {
                        DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
                    }
                },
               "Delete"
            );
            NodeActionsList.Add(deleteAction);
        }

        protected override Func<CashRequestViewModel> CreateDialogFunction => () => new CashRequestViewModel( 
            EntityUoWBuilder.ForCreate(),
            unitOfWorkFactory,
            commonServices,
            fileChooserProvider,
            employeeRepository,
            cashRepository,
            _employeeJournalFactory,
            _subdivisionJournalFactory
        );
        protected override Func<CashRequestJournalNode, CashRequestViewModel> OpenDialogFunction =>
            node => new CashRequestViewModel(
                EntityUoWBuilder.ForOpen(node.Id),
                unitOfWorkFactory,
                commonServices,
                fileChooserProvider,
                employeeRepository,
                cashRepository,
                _employeeJournalFactory,
                _subdivisionJournalFactory
            );
    }
}