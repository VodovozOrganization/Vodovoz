using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using QS.Navigation;
using Vodovoz.Journals.JournalViewModels;
using QS.Dialog.GtkUI.FileDialog;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.TempAdapters
{
	public class ComplaintsJournalFactory : IComplaintsJournalFactory
	{
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

		public ComplaintsJournalFactory(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			CreateNewDependencies();
		}

		private void CreateNewDependencies()
		{
			_employeeService = new EmployeeService();
			_fileDialogService = new FileDialogService();
			_subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
			_routeListItemRepository = new RouteListItemRepository();
			_subdivisionParametersProvider = new SubdivisionParametersProvider(new ParametersProvider());
			_gtkDlgOpener = new GtkTabsOpener();
			_userRepository = new UserRepository();
			_orderSelectorFactory = new OrderSelectorFactory();
			_employeeJournalFactory = new EmployeeJournalFactory();
			_counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());
			_deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			_complaintParametersProvider = new ComplaintParametersProvider(new ParametersProvider());
			_generalSettingsParametersProvider = new GeneralSettingsParametersProvider(new ParametersProvider());
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
				Startup.AppDIContainer.BeginLifetimeScope()
				);
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
				Startup.AppDIContainer.BeginLifetimeScope()
				);
		}
	}
}
