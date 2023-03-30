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
using QS.Tdi;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ComplaintsJournalsViewModel : TabViewModelBase
	{
		private JournalViewModelBase _journal;
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
		private readonly IUserRepository _userRepository;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelector;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IComplaintResultsRepository _complaintResultsRepository;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IComplaintParametersProvider _complaintParametersProvider;
		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;
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
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelector,
			IEmployeeSettings employeeSettings,
			IComplaintResultsRepository complaintResultsRepository,
			IComplaintParametersProvider complaintParametersProvider,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
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
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelector = nomenclatureSelector ?? throw new ArgumentNullException(nameof(nomenclatureSelector));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_complaintResultsRepository = complaintResultsRepository ?? throw new ArgumentNullException(nameof(complaintResultsRepository));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_complaintParametersProvider = complaintParametersProvider ?? throw new ArgumentNullException(nameof(complaintParametersProvider));
			_generalSettingsParametersProvider = generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			_filterViewModel.DisposeOnDestroy = false;

			Title = "2 Журнал рекламаций";
			ChangeView(typeof(ComplaintsJournalViewModel));
		}

		public override void Dispose()
		{
			_filterViewModel.Dispose();
			base.Dispose();
		}

		private void ChangeView(Type switchToType)
		{
			var newJournal = GetNewJournal(switchToType);
			UpdateJournal(newJournal);
		}

		private JournalViewModelBase GetNewJournal(Type switchToType)
		{
			if(switchToType == typeof(ComplaintsWithDepartmentsReactionJournalViewModel))
			{
				var withDepartmentsReactionComplaintsJournal = GetJournalWithDepartmentsReaction();
				withDepartmentsReactionComplaintsJournal.ChangeView += ChangeView;
				return withDepartmentsReactionComplaintsJournal;
			}

			var standartComplaintsJournal = GetJournalStandart();
			standartComplaintsJournal.ChangeView += ChangeView;
			return standartComplaintsJournal;
		}

		private void UpdateJournal(JournalViewModelBase newJournal)
		{
			Journal?.Dispose();
			Journal = newJournal;
		}

		public JournalViewModelBase Journal
		{
			get => _journal;
			set => SetField(ref _journal, value);
		}

		private ComplaintsJournalViewModel GetJournalStandart()
		{
			return new ComplaintsJournalViewModel(
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
			_subdivisionJournalFactory,
			_gtkDialogsOpener,
			_userRepository,
			_orderSelectorFactory,
			_employeeJournalFactory,
			_counterpartyJournalFactory,
			_deliveryPointJournalFactory,
			_salesPlanJournalFactory,
			_nomenclatureSelector,
			_employeeSettings,
			_complaintResultsRepository,
			_complaintParametersProvider,
			_generalSettingsParametersProvider,
			this,
			_scope);
		}

		private ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction()
		{
			return new ComplaintsWithDepartmentsReactionJournalViewModel(
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
			_subdivisionJournalFactory,
			_gtkDialogsOpener,
			_userRepository,
			_orderSelectorFactory,
			_employeeJournalFactory,
			_counterpartyJournalFactory,
			_deliveryPointJournalFactory,
			_salesPlanJournalFactory,
			_nomenclatureSelector,
			_employeeSettings,
			_complaintResultsRepository,
			_complaintParametersProvider,
			_generalSettingsParametersProvider,
			this,
			_scope);
		}
	}
}
