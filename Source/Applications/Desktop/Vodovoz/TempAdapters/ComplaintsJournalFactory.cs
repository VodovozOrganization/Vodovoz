using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class ComplaintsJournalFactory : IComplaintsJournalFactory
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly INavigationManager _navigationManager;

		private IEmployeeService _employeeService;
		private IFileDialogService _fileDialogService;
		private ISubdivisionRepository _subdivisionRepository;
		private IRouteListItemRepository _routeListItemRepository;
		private ISubdivisionParametersProvider _subdivisionParametersProvider;
		private IGtkTabsOpener _gtkDlgOpener;
		private IUserRepository _userRepository;
		private IOrderSelectorFactory _orderSelectorFactory;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ICounterpartyJournalFactory _counterpartyJournalFactory;
		private IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private IComplaintParametersProvider _complaintParametersProvider;
		private IGeneralSettingsParametersProvider _generalSettingsParametersProvider;

		public ComplaintsJournalFactory(
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IGtkTabsOpener gtkDlgOpener,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			IComplaintParametersProvider complaintParametersProvider,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_gtkDlgOpener = gtkDlgOpener ?? throw new ArgumentNullException(nameof(gtkDlgOpener));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_complaintParametersProvider = complaintParametersProvider ?? throw new ArgumentNullException(nameof(complaintParametersProvider));
			_generalSettingsParametersProvider = generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));
		}

		public ComplaintsJournalViewModel GetStandartJournal(ComplaintFilterViewModel filterViewModel)
		{
			return new ComplaintsJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_navigationManager,
				_employeeService,
				_routeListItemRepository,
				_subdivisionParametersProvider,
				filterViewModel,
				_fileDialogService,
				_subdivisionRepository,
				_gtkDlgOpener,
				_userRepository,
				_orderSelectorFactory,
				_employeeJournalFactory,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory,
				_complaintParametersProvider,
				_generalSettingsParametersProvider,
				_lifetimeScope);
		}

		public ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction(ComplaintFilterViewModel filterViewModel)
		{
			return new ComplaintsWithDepartmentsReactionJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_navigationManager,
				_employeeService,
				_routeListItemRepository,
				_subdivisionParametersProvider,
				filterViewModel,
				_fileDialogService,
				_subdivisionRepository,
				_gtkDlgOpener,
				_userRepository,
				_orderSelectorFactory,
				_employeeJournalFactory,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory,
				_complaintParametersProvider,
				_generalSettingsParametersProvider,
				_lifetimeScope);
		}
	}
}
