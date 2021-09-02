using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Project.Services.Interactive;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class CashRequestJournalViewModel : FilterableMultipleEntityJournalViewModelBase
        <CashRequestJournalNode, CashRequestJournalFilterViewModel>
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IFileChooserProvider fileChooserProvider;
        private readonly IEmployeeRepository employeeRepository;
        private readonly ICashRepository cashRepository;
        private readonly IEmployeeJournalFactory _employeeJournalFactory;
        private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
        private readonly ICommonServices _commonServices;
        private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
        private readonly IExpenseCategorySelectorFactory _expenseCategorySelectorFactory;
        private readonly IFilePickerService _filePickerService;

        private bool _isAdmin;
        private bool _cashRequestFinancier;
        private bool _cashRequestCoordinator;
        private bool _roleCashier;
        private bool _canSeeCurrentSubdivisonRequests;
        private int _currentEmployeeId;
        private Employee _currentEmployee;

        public CashRequestJournalViewModel(
            CashRequestJournalFilterViewModel filterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IFileChooserProvider fileChooserProvider,
            IEmployeeRepository employeeRepository,
            ICashRepository cashRepository,
            IEmployeeJournalFactory employeeJournalFactory,
            ISubdivisionJournalFactory subdivisionJournalFactory,
            ICounterpartyJournalFactory counterpartyJournalFactory,
            IExpenseCategorySelectorFactory expenseCategorySelectorFactory,
            IFilePickerService filePickerService
        ) : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            this.cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
            _employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            _subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
            _expenseCategorySelectorFactory = expenseCategorySelectorFactory
                                              ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory));
            _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));

            TabName = "Журнал заявок ДС";
            
            UpdateOnChanges(
                typeof(CashRequest),
                typeof(CashlessRequest),
                typeof(CashRequestSumItem),
                typeof(Subdivision),
                typeof(Employee)
            );

            RegisterCashRequest();
            RegisterCashlessRequest();

            var threadLoader = DataLoader as ThreadDataLoader<CashRequestJournalNode>;
            threadLoader?.MergeInOrderBy(x => x.Date, @descending: true);

            FinishJournalConfiguration();
            AccessRequest();
        }

        private void AccessRequest()
        {
	        var userId = _commonServices.UserService.CurrentUserId;
	        _currentEmployee = employeeRepository.GetEmployeesForUser(UoW, userId).First();
	        _currentEmployeeId = _currentEmployee.Id;

	        _isAdmin = _commonServices.UserService.GetCurrentUser(UoW).IsAdmin;
	        _cashRequestFinancier =
		        _commonServices.PermissionService.ValidateUserPresetPermission("role_financier_cash_request", userId);
	        _cashRequestCoordinator =
		        _commonServices.PermissionService.ValidateUserPresetPermission("role_coordinator_cash_request", userId);
	        _roleCashier = _commonServices.PermissionService.ValidateUserPresetPermission("role_сashier", userId);
	        _canSeeCurrentSubdivisonRequests =
		        _commonServices.CurrentPermissionService.ValidatePresetPermission("can_see_current_subdivision_cash_requests");
        }

        #region JournalActions

        protected override void CreateNodeActions()
        {
	        NodeActionsList.Clear();
	        CreateDefaultSelectAction();
	        CreateDefaultAddActions();
	        CreateDefaultEditAction();
	        CreateDeleteAction();
        }

        private void CreateDeleteAction()
        {
	        var deleteAction = new JournalAction("Удалить",
		        (selected) => {

			        var selectedNodes = selected.OfType<CashRequestJournalNode>();

			        if(selectedNodes == null || selectedNodes.Count() != 1) {
				        return false;
			        }

			        CashRequestJournalNode selectedNode = selectedNodes.First();

			        if (selectedNode.PayoutRequestState != PayoutRequestState.New){
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

        #endregion

        #region Cash
        
        private void RegisterCashRequest()
        {
	        var cashConfig = RegisterEntity(GetCashRequestQuery)
		        .AddDocumentConfiguration(
			        //функция диалога создания документа
			        () => new CashRequestViewModel(
				        EntityUoWBuilder.ForCreate(),
				        unitOfWorkFactory,
				        _commonServices,
				        fileChooserProvider,
				        employeeRepository,
				        cashRepository,
				        _employeeJournalFactory,
				        _subdivisionJournalFactory
			        ),
			        //функция диалога открытия документа
			        node => new CashRequestViewModel(
				        EntityUoWBuilder.ForOpen(node.Id),
				        unitOfWorkFactory,
				        _commonServices,
				        fileChooserProvider,
				        employeeRepository,
				        cashRepository,
				        _employeeJournalFactory,
				        _subdivisionJournalFactory
			        ),
			        //функция идентификации документа
			        node => node.EntityType == typeof(CashRequest),
			        "Заявка на выдачу наличных Д/С",
			        new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
		        );

	        //завершение конфигурации
	        cashConfig.FinishConfiguration();
        }

        private IQueryOver<CashRequest> GetCashRequestQuery(IUnitOfWork uow)
        {
            CashRequest cashRequestAlias = null;
            Employee authorAlias = null;
            CashRequestSumItem cashRequestSumItemAlias = null;
            Employee accountableEmployeeAlias = null;

            CashRequestJournalNode<CashRequest> resultAlias = null;

            var result = uow.Session.QueryOver(() => cashRequestAlias)
                .Left.JoinAlias(с => с.Sums, () => cashRequestSumItemAlias)
                .Left.JoinAlias(с => с.Author, () => authorAlias)
                .Left.JoinAlias(() => cashRequestSumItemAlias.AccountableEmployee, () => accountableEmployeeAlias);
            
            if(FilterViewModel != null)
            {
                if(FilterViewModel.StartDate.HasValue)
                {
	                result.Where(() => cashRequestAlias.Date >= FilterViewModel.StartDate.Value.Date);
                }

                if(FilterViewModel.EndDate.HasValue)
                {
	                result.Where(() => cashRequestAlias.Date < FilterViewModel.EndDate.Value.Date.AddDays(1));
                }

                if(FilterViewModel.Author != null)
                {
	                result.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
                }

                if (FilterViewModel.AccountableEmployee != null)
                {
	                result.Where(() => accountableEmployeeAlias.Id == FilterViewModel.AccountableEmployee.Id);
                }

                if (FilterViewModel.State != null )
                {
                    result.Where(() => cashRequestAlias.PayoutRequestState == FilterViewModel.State);
                }

                if(FilterViewModel.DocumentType != PayoutRequestDocumentType.CashRequest &&
                   FilterViewModel.DocumentType != null)
                {
	                result.Where(cr => cr.Id == -1);
                }
            }

            if (!_isAdmin && !_cashRequestFinancier && !_cashRequestCoordinator && !_roleCashier)
            {
	            if (_canSeeCurrentSubdivisonRequests)
	            {
		            result.Where(() => authorAlias.Subdivision.Id == _currentEmployee.Subdivision.Id);
	            }
	            else
	            {
		            result.Where(() => cashRequestAlias.Author.Id == _currentEmployeeId);
	            }
            }

            var authorProjection = CustomProjections.Concat_WS(" ",
                Projections.Property(() => authorAlias.LastName),
                Projections.Property(() => authorAlias.Name),
                Projections.Property(() => authorAlias.Patronymic)
            );
            
            var accauntableProjection = CustomProjections.Concat_WS(" ",
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
                    .Select(c => c.PayoutRequestState).WithAlias(() => resultAlias.PayoutRequestState)
                    .Select(c => c.PayoutRequestDocumentType).WithAlias(() => resultAlias.PayoutRequestDocumentType)
                    .Select(authorProjection).WithAlias(() => resultAlias.Author)
                    .Select(accauntableProjection).WithAlias(() => resultAlias.AccountablePerson)
                    .SelectSubQuery(cashReuestSumSubquery).WithAlias(() => resultAlias.Sum)
                    .Select(c => c.Basis).WithAlias(() => resultAlias.Basis)
                ).TransformUsing(Transformers.AliasToBean<CashRequestJournalNode<CashRequest>>())
                .OrderBy(x => x.Date).Desc();
            return result;
        }

        #endregion

        #region Cashless

        private void RegisterCashlessRequest()
        {
	        var cashlessConfig = RegisterEntity(GetCashlessRequestQuery)
		        .AddDocumentConfiguration(
			        //функция диалога создания документа
			        () => new CashlessRequestViewModel(
				        _filePickerService,
				        _expenseCategorySelectorFactory,
				        new UserRepository(),
				        _counterpartyJournalFactory,
				        new EmployeeRepository(),
				        EntityUoWBuilder.ForCreate(),
				        unitOfWorkFactory,
				        _commonServices
			        ),
			        //функция диалога открытия документа
			        node => new CashlessRequestViewModel(
				        _filePickerService,
				        _expenseCategorySelectorFactory,
				        new UserRepository(),
				        _counterpartyJournalFactory,
				        new EmployeeRepository(),
				        EntityUoWBuilder.ForOpen(node.Id),
				        unitOfWorkFactory,
				        _commonServices
			        ),
			        //функция идентификации документа
			        node => node.EntityType == typeof(CashlessRequest),
			        "Заявка на оплату по Б/Н",
			        new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
		        );

	        //завершение конфигурации
	        cashlessConfig.FinishConfiguration();
        }

        private IQueryOver<CashlessRequest> GetCashlessRequestQuery(IUnitOfWork uow)
        {
            CashlessRequest cashlessRequestAlias = null;
            Employee authorAlias = null;
            Counterparty counterpartyAlias = null;

            CashRequestJournalNode<CashlessRequest> resultAlias = null;

            var result = uow.Session.QueryOver(() => cashlessRequestAlias)
	            .Left.JoinAlias(с => с.Author, () => authorAlias)
	            .Left.JoinAlias(c => c.Counterparty, () => counterpartyAlias);

            if(FilterViewModel != null)
            {
                if(FilterViewModel.StartDate.HasValue)
                {
	                result.Where(() => cashlessRequestAlias.Date >= FilterViewModel.StartDate.Value.Date);
                }

                if(FilterViewModel.EndDate.HasValue)
                {
	                result.Where(() => cashlessRequestAlias.Date < FilterViewModel.EndDate.Value.Date.AddDays(1));
                }

                if(FilterViewModel.Author != null)
                {
	                result.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
                }

                if (FilterViewModel.State != null )
                {
                    result.Where(() => cashlessRequestAlias.PayoutRequestState == FilterViewModel.State);
                }

                if(FilterViewModel.DocumentType != PayoutRequestDocumentType.CashlessRequest &&
                   FilterViewModel.DocumentType != null)
                {
	                result.Where(clr => clr.Id == -1);
                }

                if (FilterViewModel.Counterparty != null )
                {
	                result.Where(() => cashlessRequestAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
                }
            }

            if (!_isAdmin && !_cashRequestFinancier && !_cashRequestCoordinator && !_roleCashier)
            {
	            if (_canSeeCurrentSubdivisonRequests)
	            {
		            result.Where(() => authorAlias.Subdivision.Id == _currentEmployee.Subdivision.Id);
	            }
	            else
	            {
		            result.Where(() => cashlessRequestAlias.Author.Id == _currentEmployeeId);
	            }
            }

            var authorProjection = CustomProjections.Concat_WS(" ",
                Projections.Property(() => authorAlias.LastName),
                Projections.Property(() => authorAlias.Name),
                Projections.Property(() => authorAlias.Patronymic)
            );

            result.Where(GetSearchCriterion(
                () => authorAlias.Id,
                () => authorProjection,
                () => cashlessRequestAlias.Basis
            ));

            result.SelectList(list => list
                    .SelectGroup(clr => clr.Id)
                    .Select(clr => clr.Id).WithAlias(() => resultAlias.Id)
                    .Select(clr => clr.Date).WithAlias(() => resultAlias.Date)
                    .Select(clr => clr.PayoutRequestState).WithAlias(() => resultAlias.PayoutRequestState)
                    .Select(clr => clr.PayoutRequestDocumentType).WithAlias(() => resultAlias.PayoutRequestDocumentType)
                    .Select(authorProjection).WithAlias(() => resultAlias.Author)
                    .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
                    .Select(clr => clr.Basis).WithAlias(() => resultAlias.Basis)
                    .Select(clr => clr.Sum).WithAlias(() => resultAlias.Sum)
                ).TransformUsing(Transformers.AliasToBean<CashRequestJournalNode<CashlessRequest>>())
                .OrderBy(clr => clr.Date).Desc();
            return result;
        }

        #endregion
    }
}
