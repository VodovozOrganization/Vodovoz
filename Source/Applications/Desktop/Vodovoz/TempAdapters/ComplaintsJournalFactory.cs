using QS.Tdi;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Collections;
using System.Linq;
using Autofac;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport;
using Order = Vodovoz.Domain.Orders.Order;
using QS.Navigation;
using Vodovoz.Journals.JournalViewModels;
using QS.Dialog.GtkUI.FileDialog;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
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
		private ISubdivisionJournalFactory _subdivisionJournalFactory;
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
			_subdivisionJournalFactory = new SubdivisionJournalFactory();
			_gtkDlgOpener = new GtkTabsOpener();
			_userRepository = new UserRepository();
			_orderSelectorFactory = new OrderSelectorFactory();
			_employeeJournalFactory = new EmployeeJournalFactory();
			_counterpartyJournalFactory = new CounterpartyJournalFactory();
			_deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			_complaintParametersProvider = new ComplaintParametersProvider(new ParametersProvider());
			_generalSettingsParametersProvider = new GeneralSettingsParametersProvider(new ParametersProvider());
		}

		public ComplaintsJournalViewModel GetStandartJournal(ComplaintFilterViewModel filterViewModel, ITdiTab parentDialog)
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
				_subdivisionJournalFactory,
				_gtkDlgOpener,
				_userRepository,
				_orderSelectorFactory,
				_employeeJournalFactory,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory,
				_complaintParametersProvider,
				_generalSettingsParametersProvider,
				MainClass.AppDIContainer.BeginLifetimeScope(),
				parentDialog
				);
		}

		public ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction(ComplaintFilterViewModel filterViewModel, ITdiTab parentDialog)
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
				_subdivisionJournalFactory,
				_gtkDlgOpener,
				_userRepository,
				_orderSelectorFactory,
				_employeeJournalFactory,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory,
				_complaintParametersProvider,
				_generalSettingsParametersProvider,
				MainClass.AppDIContainer.BeginLifetimeScope(),
				parentDialog
				);
		}
	}
}
