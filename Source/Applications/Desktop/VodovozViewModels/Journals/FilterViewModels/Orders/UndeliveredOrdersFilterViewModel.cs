using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveredOrdersFilterViewModel : FilterViewModelBase<UndeliveredOrdersFilterViewModel>
	{
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

		public UndeliveredOrdersFilterViewModel(ICommonServices commonServices, IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory, ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory, ISubdivisionJournalFactory subdivisionJournalFactory)
		{
			OrderSelectorFactory = (orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory)))
				.CreateOrderAutocompleteSelectorFactory();

			DriverEmployeeSelectorFactory = (employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			OfficeEmployeeSelectorFactory = employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();

			CounterpartySelectorFactory = (counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();

			DeliveryPointSelectorFactory = (deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory)))
				.CreateDeliveryPointAutocompleteSelectorFactory();

			AuthorSubdivisionSelectorFactory = (subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)))
				.CreateDefaultSubdivisionAutocompleteSelectorFactory(employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());

			Subdivisions = UoW.GetAll<Subdivision>();
			RestrictOldOrderStartDate = DateTime.Today.AddMonths(-1);
			RestrictOldOrderEndDate = DateTime.Today.AddMonths(1);
		}

		public IEntityAutocompleteSelectorFactory OrderSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverEmployeeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory OfficeEmployeeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DeliveryPointSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory AuthorSubdivisionSelectorFactory { get; }

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
	}
}
