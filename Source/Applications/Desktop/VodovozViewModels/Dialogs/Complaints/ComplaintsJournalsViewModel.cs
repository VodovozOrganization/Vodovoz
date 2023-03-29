using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using Autofac;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ComplaintsJournalsViewModel : TabViewModelBase
	{
		private FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel> _journal;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private readonly IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
		private readonly IEmployeeService _employeeService;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly ComplaintFilterViewModel _filterViewModel;
		private readonly IFileDialogService _fileDialogService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelector;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IComplaintParametersProvider _complaintParametersProvider;
		private readonly ILifetimeScope _scope;

		public ComplaintsJournalsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			IEmployeeService employeeService,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			ComplaintFilterViewModel filterViewModel,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IGtkTabsOpener gtkDialogsOpener,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelector,
			IEmployeeSettings employeeSettings,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IComplaintParametersProvider complaintParametersProvider,
			ILifetimeScope scope) : base(commonServices.InteractiveService, navigationManager)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_undeliveredOrdersJournalOpener = undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_gtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelector = nomenclatureSelector ?? throw new ArgumentNullException(nameof(nomenclatureSelector));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_complaintParametersProvider = complaintParametersProvider ?? throw new ArgumentNullException(nameof(complaintParametersProvider));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			Title = "2 Журнал рекламаций";

			ChangeView(typeof(ComplaintsJournalViewModel));
		}

		private void ChangeView(Type switchToType)
		{
			UpdateJournal(switchToType);
		}

		private void UpdateJournal(Type switchToType)
		{
			Journal?.Dispose();
			if(switchToType == typeof(ComplaintsJournalViewModel))
			{
				var standartComplaintsJournal = new ComplaintsJournalViewModel(
				_unitOfWorkFactory,
				_commonServices,
				_navigationManager,
				_undeliveredOrdersJournalOpener,
				_employeeService,
				_routeListItemRepository,
				_subdivisionParametersProvider,
				_filterViewModel,
				_fileDialogService,
				_subdivisionRepository,
				_gtkDialogsOpener,
				_nomenclatureRepository,
				_userRepository,
				_orderSelectorFactory,
				_employeeJournalFactory,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory,
				_subdivisionJournalFactory,
				_salesPlanJournalFactory,
				_nomenclatureSelector,
				_employeeSettings,
				_undeliveredOrdersRepository,
				_complaintParametersProvider,
				_scope);

				standartComplaintsJournal.ChangeView += ChangeView;
				Journal = standartComplaintsJournal;
			}
			else
			{
				var withDepartmentsReactionComplaintsJournal = new ComplaintsWithDepartmentsReactionJournalViewModel(
				_unitOfWorkFactory,
				_commonServices,
				_navigationManager,
				_undeliveredOrdersJournalOpener,
				_employeeService,
				_routeListItemRepository,
				_subdivisionParametersProvider,
				_filterViewModel,
				_fileDialogService,
				_subdivisionRepository,
				_gtkDialogsOpener,
				_nomenclatureRepository,
				_userRepository,
				_orderSelectorFactory,
				_employeeJournalFactory,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory,
				_subdivisionJournalFactory,
				_salesPlanJournalFactory,
				_nomenclatureSelector,
				_employeeSettings,
				_undeliveredOrdersRepository,
				_complaintParametersProvider,
				_scope);

				withDepartmentsReactionComplaintsJournal.ChangeView += ChangeView;
				Journal = withDepartmentsReactionComplaintsJournal;
			}
		}

		public FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel> Journal
		{
			get => _journal;
			set => SetField(ref _journal, value);
		}
	}
}
