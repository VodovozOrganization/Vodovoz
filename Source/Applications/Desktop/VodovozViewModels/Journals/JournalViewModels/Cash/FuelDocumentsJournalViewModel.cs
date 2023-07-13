using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Controllers;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.Nodes.Cash;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class FuelDocumentsJournalViewModel : MultipleEntityJournalViewModelBase<FuelDocumentJournalNode>
    {
	    private readonly ICommonServices _commonServices;
	    private readonly IEmployeeService _employeeService;
	    private readonly ISubdivisionRepository _subdivisionRepository;
	    private readonly IFuelRepository _fuelRepository;
	    private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
	    private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
	    private readonly IEmployeeJournalFactory _employeeJournalFactory;
	    private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
	    private readonly ICarJournalFactory _carJournalFactory;
	    private readonly IReportViewOpener _reportViewOpener;
	    private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly ILifetimeScope _lifetimeScope;

		public FuelDocumentsJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IEmployeeService employeeService,
            ISubdivisionRepository subdivisionRepository,
            IFuelRepository fuelRepository,
            ICounterpartyJournalFactory counterpartyJournalFactory,
            INomenclatureJournalFactory nomenclatureSelectorFactory,
            IEmployeeJournalFactory employeeJournalFactory,
            ISubdivisionJournalFactory subdivisionJournalFactory,
            ICarJournalFactory carJournalFactory,
            IReportViewOpener reportViewOpener,
            IRouteListProfitabilityController routeListProfitabilityController,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope) : base(unitOfWorkFactory, commonServices)
        {
	        _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
	        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
	        _subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
	        _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
	        _counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
	        _nomenclatureSelectorFactory =
		        nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
	        _employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
	        _subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
	        _carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
	        _reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
	        _routeListProfitabilityController =
		        routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			TabName = "Журнал учета топлива";

			var loader = new ThreadDataLoader<FuelDocumentJournalNode>(unitOfWorkFactory);
			loader.MergeInOrderBy(x => x.CreationDate, true);
			DataLoader = loader;
		        
            RegisterIncomeInvoice();
            RegisterTransferDocument();
            RegisterWriteoffDocument();
            
            FinishJournalConfiguration();
            
            UpdateOnChanges(
            	typeof(FuelIncomeInvoice),
            	typeof(FuelIncomeInvoiceItem),
            	typeof(FuelTransferDocument),
            	typeof(FuelWriteoffDocument),
            	typeof(FuelWriteoffDocumentItem)
            );
        }

	    public override string FooterInfo {
		    get {
			    var balance = _fuelRepository.GetAllFuelsBalance(UoW);
			    string result = "";
			    foreach (var item in balance) {
				    result += $"{item.Key.Name}: {item.Value.ToString("0")} л., ";
			    }
			    result.Trim(' ', ',');
			    return result;
		    }
	    }

		public INavigationManager NavigationManager { get; }

		#region IncomeInvoice

		private IQueryOver<FuelIncomeInvoice> GetFuelIncomeQuery(IUnitOfWork uow)
	    {
		    FuelDocumentJournalNode resultAlias = null;
		    FuelIncomeInvoice fuelIncomeInvoiceAlias = null;
		    FuelIncomeInvoiceItem fuelIncomeInvoiceItemAlias = null;
		    Employee authorAlias = null;
		    Subdivision subdivisionToAlias = null;
		    var fuelIncomeInvoiceQuery = uow.Session.QueryOver<FuelIncomeInvoice>(() => fuelIncomeInvoiceAlias)
			    .Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.Author, () => authorAlias)
			    .Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.Subdivision, () => subdivisionToAlias)
			    .Left.JoinQueryOver(() => fuelIncomeInvoiceAlias.FuelIncomeInvoiceItems,
				    () => fuelIncomeInvoiceItemAlias)
			    .SelectList(list => list
				    .SelectGroup(() => fuelIncomeInvoiceAlias.Id).WithAlias(() => resultAlias.Id)
				    .Select(() => fuelIncomeInvoiceAlias.СreationTime).WithAlias(() => resultAlias.CreationDate)
				    .Select(() => fuelIncomeInvoiceAlias.Comment).WithAlias(() => resultAlias.Comment)
				    .Select(Projections.Sum(Projections.Property(() => fuelIncomeInvoiceItemAlias.Liters)))
				    .WithAlias(() => resultAlias.Liters)
				    .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				    .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
				    .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

				    .Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
			    )
			    .OrderBy(() => fuelIncomeInvoiceAlias.СreationTime).Desc()
			    .TransformUsing(Transformers.AliasToBean<FuelDocumentJournalNode<FuelIncomeInvoice>>());

		    fuelIncomeInvoiceQuery.Where(GetSearchCriterion(
			    () => authorAlias.Name,
			    () => authorAlias.LastName,
			    () => authorAlias.Patronymic,
			    () => fuelIncomeInvoiceAlias.Comment
			));

		    return fuelIncomeInvoiceQuery;
	    }
        
	    private void RegisterIncomeInvoice()
	    {
		    var complaintConfig = RegisterEntity<FuelIncomeInvoice>(GetFuelIncomeQuery)
			    .AddDocumentConfiguration(
				    //функция диалога создания документа
				    () => new FuelIncomeInvoiceViewModel(
					    EntityUoWBuilder.ForCreate(),
					    UnitOfWorkFactory,
					    _employeeService,
					    _nomenclatureSelectorFactory,
					    _subdivisionRepository,
					    _fuelRepository,
					    _counterpartyJournalFactory,
					    _commonServices
				    ),
				    //функция диалога открытия документа
				    (FuelDocumentJournalNode node) => new FuelIncomeInvoiceViewModel(
					    EntityUoWBuilder.ForOpen(node.Id),
					    UnitOfWorkFactory,
					    _employeeService,
					    _nomenclatureSelectorFactory,
					    _subdivisionRepository,
					    _fuelRepository,
					    _counterpartyJournalFactory,
					    _commonServices
				    ),
				    //функция идентификации документа 
				    (FuelDocumentJournalNode node) => {
					    return node.EntityType == typeof(FuelIncomeInvoice);
				    },
				    "Входящая накладная"
			    );

		    //завершение конфигурации
		    complaintConfig.FinishConfiguration();
	    }

	    #endregion IncomeInvoice

	    #region TransferDocument

	    private IQueryOver<FuelTransferDocument> GetTransferDocumentQuery(IUnitOfWork uow)
	    {
		    FuelDocumentJournalNode resultAlias = null;
		    FuelTransferDocument fuelTransferAlias = null;
		    Employee authorAlias = null;
		    Subdivision subdivisionFromAlias = null;
		    Subdivision subdivisionToAlias = null;
		    var fuelTransferQuery = uow.Session.QueryOver<FuelTransferDocument>(() => fuelTransferAlias)
			    .Left.JoinQueryOver(() => fuelTransferAlias.Author, () => authorAlias)
			    .Left.JoinQueryOver(() => fuelTransferAlias.CashSubdivisionFrom, () => subdivisionFromAlias)
			    .Left.JoinQueryOver(() => fuelTransferAlias.CashSubdivisionTo, () => subdivisionToAlias)
			    .SelectList(list => list
				    .Select(() => fuelTransferAlias.Id).WithAlias(() => resultAlias.Id)
				    .Select(() => fuelTransferAlias.CreationTime).WithAlias(() => resultAlias.CreationDate)
				    .Select(() => fuelTransferAlias.Status).WithAlias(() => resultAlias.TransferDocumentStatus)
				    .Select(() => fuelTransferAlias.TransferedLiters).WithAlias(() => resultAlias.Liters)
				    .Select(() => fuelTransferAlias.Comment).WithAlias(() => resultAlias.Comment)
				    .Select(() => fuelTransferAlias.SendTime).WithAlias(() => resultAlias.SendTime)
				    .Select(() => fuelTransferAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)

				    .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				    .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
				    .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

				    .Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
				    .Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
			    )
			    .OrderBy(() => fuelTransferAlias.CreationTime).Desc()
			    .TransformUsing(Transformers.AliasToBean<FuelDocumentJournalNode<FuelTransferDocument>>());

		    fuelTransferQuery.Where(GetSearchCriterion(
			    () => authorAlias.Name,
			    () => authorAlias.LastName,
			    () => authorAlias.Patronymic,
			    () => fuelTransferAlias.Comment
		    ));
		    
		    return fuelTransferQuery;
	    }
        
	    private void RegisterTransferDocument()
	    {
		    var complaintConfig = RegisterEntity<FuelTransferDocument>(GetTransferDocumentQuery)
			    .AddDocumentConfiguration(
				    //функция диалога создания документа
				    () => new FuelTransferDocumentViewModel(
					    EntityUoWBuilder.ForCreate(),
					    UnitOfWorkFactory,
					    _employeeService,
					    _subdivisionRepository,
					    _fuelRepository,
					    _commonServices,
					    _employeeJournalFactory,
					    _carJournalFactory,
					    _reportViewOpener
				    ),
				    //функция диалога открытия документа
				    (FuelDocumentJournalNode node) => new FuelTransferDocumentViewModel(
					    EntityUoWBuilder.ForOpen(node.Id),
					    UnitOfWorkFactory,
					    _employeeService,
					    _subdivisionRepository,
					    _fuelRepository,
					    _commonServices,
					    _employeeJournalFactory,
					    _carJournalFactory,
					    _reportViewOpener
				    ),
				    //функция идентификации документа 
				    (FuelDocumentJournalNode node) => {
					    return node.EntityType == typeof(FuelTransferDocument);
				    },
				    "Перемещение"
			    );

		    //завершение конфигурации
		    complaintConfig.FinishConfiguration();
	    }

	    #endregion TransferDocument

	    #region WriteoffDocument

	    private IQueryOver<FuelWriteoffDocument> GetWriteoffDocumentQuery(IUnitOfWork uow)
	    {
		    FuelDocumentJournalNode resultAlias = null;
		    FuelWriteoffDocument fuelWriteoffAlias = null;
			Employee cashierAlias = null;
			Employee employeeAlias = null;
			Subdivision subdivisionAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;
			FuelWriteoffDocumentItem fuelWriteoffItemAlias = null;
			var fuelWriteoffQuery = uow.Session.QueryOver<FuelWriteoffDocument>(() => fuelWriteoffAlias)
				.Left.JoinAlias(() => fuelWriteoffAlias.Cashier, () => cashierAlias)
				.Left.JoinAlias(() => fuelWriteoffAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => fuelWriteoffAlias.CashSubdivision, () => subdivisionAlias)
				.JoinEntityAlias(
						() => financialExpenseCategoryAlias,
						() => fuelWriteoffAlias.ExpenseCategoryId == financialExpenseCategoryAlias.Id,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => fuelWriteoffAlias.FuelWriteoffDocumentItems, () => fuelWriteoffItemAlias)
				.SelectList(list => list
					.SelectGroup(() => fuelWriteoffAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fuelWriteoffAlias.Date).WithAlias(() => resultAlias.CreationDate)
					.Select(() => fuelWriteoffAlias.Reason).WithAlias(() => resultAlias.Comment)

					.Select(() => cashierAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => cashierAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => cashierAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)

					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.ExpenseCategory)
					.Select(Projections.Sum(Projections.Property(() => fuelWriteoffItemAlias.Liters))).WithAlias(() => resultAlias.Liters)

					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
				)
				.OrderBy(() => fuelWriteoffAlias.Date).Desc()
				.TransformUsing(Transformers.AliasToBean<FuelDocumentJournalNode<FuelWriteoffDocument>>());

			fuelWriteoffQuery.Where(GetSearchCriterion(
				() => cashierAlias.Name,
				() => cashierAlias.LastName,
				() => cashierAlias.Patronymic,
				() => employeeAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.Patronymic,
				() => fuelWriteoffAlias.Reason
			));
			
		    return fuelWriteoffQuery;
	    }
        
	    private void RegisterWriteoffDocument()
	    {
		    var complaintConfig = RegisterEntity<FuelWriteoffDocument>(GetWriteoffDocumentQuery)
			    .AddDocumentConfiguration(
				    //функция диалога создания документа
				    () => new FuelWriteoffDocumentViewModel(
					    EntityUoWBuilder.ForCreate(),
					    UnitOfWorkFactory,
					    _employeeService,
					    _fuelRepository,
					    _subdivisionRepository,
					    _commonServices,
					    _employeeJournalFactory,
					    _reportViewOpener,
					    _subdivisionJournalFactory,
					    _routeListProfitabilityController,
						NavigationManager,
						_lifetimeScope),
				    //функция диалога открытия документа
				    (FuelDocumentJournalNode node) => new FuelWriteoffDocumentViewModel(
					    EntityUoWBuilder.ForOpen(node.Id),
					    UnitOfWorkFactory,
					    _employeeService,
					    _fuelRepository,
					    _subdivisionRepository,
					    _commonServices,
					    _employeeJournalFactory,
					    _reportViewOpener,
					    _subdivisionJournalFactory,
					    _routeListProfitabilityController,
						NavigationManager,
						_lifetimeScope),
				    //функция идентификации документа 
				    (FuelDocumentJournalNode node) => {
					    return node.EntityType == typeof(FuelWriteoffDocument);
				    },
				    "Акт выдачи топлива"
			    );

		    //завершение конфигурации
		    complaintConfig.FinishConfiguration();
	    }

	    #endregion WriteoffDocument
    }
}
