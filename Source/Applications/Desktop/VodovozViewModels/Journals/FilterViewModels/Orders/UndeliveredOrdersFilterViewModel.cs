using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveredOrdersFilterViewModel : FilterViewModelBase<UndeliveredOrdersFilterViewModel>
	{
		private readonly INavigationManager _navigationManager;
		private ILifetimeScope _lifetimeScope;
		private Order _restrictOldOrder;
		private Employee _restrictDriver;
		private Subdivision _restrictAuthorSubdivision;
		private Counterparty _restrictClient;
		private DeliveryPoint _restrictAddress;
		private Employee _restrictOldOrderAuthor;
		private DateTime? _restrictOldOrderStartDate;
		private DateTime? _restrictOldOrderEndDate;
		private DateTime? _restrictNewOrderStartDate;
		private DateTime? _restrictNewOrderEndDate;
		private GuiltyTypes? _restrictGuiltySide;
		private Subdivision _restrictGuiltyDepartment;
		private Subdivision _restrictInProcessAtDepartment;
		private UndeliveryStatus? _restrictUndeliveryStatus;
		private Employee _restrictUndeliveryAuthor;
		private bool _restrictIsProblematicCases;
		private bool _restrictNotIsProblematicCases;
		private ActionsWithInvoice? _restrictActionsWithInvoice;
		private OrderStatus? _oldOrderStatus;
		private bool _restrictGuiltyDepartmentVisible;
		private bool? _isForSalesDepartment;
		private DialogViewModelBase _journalViewModel;

		public UndeliveredOrdersFilterViewModel(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			OrderSelectorFactory = (orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory)))
				.CreateOrderAutocompleteSelectorFactory();

			DriverEmployeeSelectorFactory = (employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			CounterpartySelectorFactory = (counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);

			DeliveryPointSelectorFactory = (deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory)))
				.CreateDeliveryPointAutocompleteSelectorFactory();

			Subdivisions = UoW.GetAll<Subdivision>();
			RestrictOldOrderStartDate = DateTime.Today.AddMonths(-1);
			RestrictOldOrderEndDate = DateTime.Today.AddMonths(1);
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public IEntityAutocompleteSelectorFactory OrderSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverEmployeeSelectorFactory { get; }
		public IEntityEntryViewModel OldOrderAuthorViewModel { get; private set; }
		public IEntityEntryViewModel UndeliveryAuthorViewModel { get; private set; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DeliveryPointSelectorFactory { get; }
		public IEntityEntryViewModel AuthorSubdivisionViewModel { get; private set; }
		public IEntityEntryViewModel InProcessAtSubdivisionViewModel { get; private set; }

		public Order RestrictOldOrder
		{
			get => _restrictOldOrder;
			set => UpdateFilterField(ref _restrictOldOrder, value);
		}

		public Employee RestrictDriver
		{
			get => _restrictDriver;
			set => UpdateFilterField(ref _restrictDriver, value);
		}

		public Subdivision RestrictAuthorSubdivision
		{
			get => _restrictAuthorSubdivision;
			set => UpdateFilterField(ref _restrictAuthorSubdivision, value);
		}

		public Counterparty RestrictClient
		{
			get => _restrictClient;
			set => UpdateFilterField(ref _restrictClient, value);
		}

		public DeliveryPoint RestrictAddress
		{
			get => _restrictAddress;
			set => UpdateFilterField(ref _restrictAddress, value);
		}

		public Employee RestrictOldOrderAuthor
		{
			get => _restrictOldOrderAuthor;
			set => UpdateFilterField(ref _restrictOldOrderAuthor, value);
		}

		public DateTime? RestrictOldOrderStartDate
		{
			get => _restrictOldOrderStartDate;
			set => UpdateFilterField(ref _restrictOldOrderStartDate, value);
		}

		public DateTime? RestrictOldOrderEndDate
		{
			get => _restrictOldOrderEndDate;
			set => UpdateFilterField(ref _restrictOldOrderEndDate, value);
		}

		public DateTime? RestrictNewOrderStartDate
		{
			get => _restrictNewOrderStartDate;
			set => UpdateFilterField(ref _restrictNewOrderStartDate, value);
		}

		public DateTime? RestrictNewOrderEndDate
		{
			get => _restrictNewOrderEndDate;
			set => UpdateFilterField(ref _restrictNewOrderEndDate, value);
		}

		public GuiltyTypes? RestrictGuiltySide
		{
			get => _restrictGuiltySide;
			set
			{
				if(value == GuiltyTypes.Department)
				{
					RestrictGuiltyDepartmentVisible = true;
				}
				else
				{
					RestrictGuiltyDepartmentVisible = false;
					RestrictGuiltyDepartment = null;
				}
				UpdateFilterField(ref _restrictGuiltySide, value);
			}
		}

		public bool RestrictGuiltyDepartmentVisible
		{
			get => _restrictGuiltyDepartmentVisible;
			set => UpdateFilterField(ref _restrictGuiltyDepartmentVisible, value);
		}

		public Subdivision RestrictGuiltyDepartment
		{
			get => _restrictGuiltyDepartment;
			set => UpdateFilterField(ref _restrictGuiltyDepartment, value);
		}

		public Subdivision RestrictInProcessAtDepartment
		{
			get => _restrictInProcessAtDepartment;
			set => UpdateFilterField(ref _restrictInProcessAtDepartment, value);
		}

		public ActionsWithInvoice? RestrictActionsWithInvoice
		{
			get => _restrictActionsWithInvoice;
			set
			{
				switch(value)
				{
					case ActionsWithInvoice.createdNew:
						NewInvoiceCreated = true;
						break;
					case ActionsWithInvoice.notCreated:
						NewInvoiceCreated = false;
						break;
					default:
						NewInvoiceCreated = null;
						break;
				}
				UpdateFilterField(ref _restrictActionsWithInvoice, value);
			}
		}

		public OrderStatus? OldOrderStatus
		{
			get => _oldOrderStatus;
			set => UpdateFilterField(ref _oldOrderStatus, value);
		}

		public bool? NewInvoiceCreated { get; set; }

		public UndeliveryStatus? RestrictUndeliveryStatus
		{
			get => _restrictUndeliveryStatus;
			set => UpdateFilterField(ref _restrictUndeliveryStatus, value);
		}

		public Employee RestrictUndeliveryAuthor
		{
			get => _restrictUndeliveryAuthor;
			set => UpdateFilterField(ref _restrictUndeliveryAuthor, value);
		}

		public bool RestrictIsProblematicCases
		{
			get => _restrictIsProblematicCases;
			set
			{
				if(value)
				{
					RestrictGuiltySide = null;
				}

				RestrictNotIsProblematicCases = !value;

				UpdateFilterField(ref _restrictIsProblematicCases, value);
			}
		}

		public bool RestrictNotIsProblematicCases
		{
			get => _restrictNotIsProblematicCases;
			set => UpdateFilterField(ref _restrictNotIsProblematicCases, value);
		}

		public bool? IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set => UpdateFilterField(ref _isForSalesDepartment, value);
		}

		public IEnumerable<Subdivision> Subdivisions { get; private set; }

		public GuiltyTypes[] ExcludingGuiltiesForProblematicCases => new GuiltyTypes[] { GuiltyTypes.Client, GuiltyTypes.None };

		public DialogViewModelBase JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;


				var inProcessAtsubdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<UndeliveredOrdersFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope);

				InProcessAtSubdivisionViewModel = inProcessAtsubdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.RestrictInProcessAtDepartment)
					.UseViewModelDialog<SubdivisionViewModel>()
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
						filter =>
						{
						})
					.Finish();

				var oldOrderAuthorSubdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<UndeliveredOrdersFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope);

				OldOrderAuthorViewModel = oldOrderAuthorSubdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.RestrictOldOrderAuthor)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
						})
					.Finish();

				var undeliveryAuthorSubdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<UndeliveredOrdersFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope);

				UndeliveryAuthorViewModel = undeliveryAuthorSubdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.RestrictUndeliveryAuthor)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
						})
					.Finish();

				var authorSubdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<UndeliveredOrdersFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope);

				AuthorSubdivisionViewModel = authorSubdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.RestrictAuthorSubdivision)
					.UseViewModelDialog<SubdivisionViewModel>()
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
						filter =>
						{
						})
					.Finish();
			}
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			_journalViewModel = null;
			base.Dispose();
		}
	}
}
