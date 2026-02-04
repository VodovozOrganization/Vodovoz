using Autofac;
using Gamma.Widgets;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Gamma.Widgets;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.Widgets.Search;

namespace Vodovoz.Filters.ViewModels
{
	public class OrderJournalFilterViewModel : FilterViewModelBase<OrderJournalFilterViewModel>
	{
		#region Поля
		
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private PaymentType[] _allowPaymentTypes;
		private OrderStatus[] _allowStatuses;
		private object[] _hideStatuses;
		private bool? _isForRetail;
		private bool? _isForSalesDepartment;
		private OrderPaymentStatus? _orderPaymentStatus;
		private Organization _organisation;
		private PaymentFrom _paymentByCardFrom;
		private PaymentOrder? _paymentOrder;
		private bool _paymentsFromVisibility;
		private Counterparty _counterparty;
		private DateTime? _restrictEndDate;
		private bool? _restrictHideService;
		private bool? _restrictLessThreeHours;
		private bool? _restrictOnlySelfDelivery;
		private bool? _restrictOnlyService;
		private PaymentType? _restrictPaymentType;
		private DateTime? _restrictStartDate;
		private OrderStatus? _restrictStatus;
		private bool? _restrictWithoutSelfDelivery;
		private ViewTypes _viewTypes;
		private bool _canChangeDeliveryPoint = true;
		private bool _canChangeSalesManager = true;
		private DeliveryPoint _deliveryPoint;
		private Employee _author;
		private Employee _salesManager;
		private int? _orderId;
		private int? _onlineOrderId;
		private string _counterpartyPhone;
		private string _deliveryPointPhone;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private OrdersDateFilterType? _restrictFilterDateType;
		private OrdersDateFilterType _filterDateType = OrdersDateFilterType.DeliveryDate;
		private IEnumerable<Organization> _organisations;
		private IEnumerable<PaymentFrom> _paymentsFrom;
		private IEnumerable<GeoGroup> _geographicGroups;
		private bool? _filterClosingDocumentDeliverySchedule;
		private string _counterpartyInn;
		private readonly CompositeSearchViewModel _searchByAddressViewModel;
		private ILifetimeScope _lifetimeScope;
		private object _edoDocFlowStatus;
			
		#endregion

		public OrderJournalFilterViewModel(
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ILifetimeScope lifetimeScope,
			IEmployeeJournalFactory employeeJournalFactory)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_deliveryPointJournalFilterViewModel = new DeliveryPointJournalFilterViewModel();
			deliveryPointJournalFactory?.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilterViewModel);
			DeliveryPointSelectorFactory = deliveryPointJournalFactory?.CreateDeliveryPointByClientAutocompleteSelectorFactory()
										   ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));

			CounterpartySelectorFactory = counterpartyJournalFactory?.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope)
										  ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			
			ManagerSelectorFactory = employeeJournalFactory?.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory() 
			                         ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			
			_searchByAddressViewModel = new CompositeSearchViewModel();
			_searchByAddressViewModel.OnSearch += OnSearchByAddressViewModel;

			EdoDocFlowStatus = SpecialComboState.All;

			StartDate = DateTime.Now.AddMonths(-1);
			EndDate = DateTime.Now.AddDays(7);
		}

		#region Автосвойства
		public int[] IncludeDistrictsIds { get; set; }
		public int[] ExceptIds { get; set; }
		public IEnumerable<Organization> Organisations => _organisations ?? (_organisations = UoW.GetAll<Organization>().ToList());
		public IEnumerable<PaymentFrom> PaymentsFrom => _paymentsFrom ?? (_paymentsFrom = UoW.GetAll<PaymentFrom>().ToList());
		public virtual IEntityAutocompleteSelectorFactory DeliveryPointSelectorFactory { get; }
		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public virtual IEntityAutocompleteSelectorFactory ManagerSelectorFactory { get; }
		public IEntityEntryViewModel AuthorViewModel { get; private set; }

		#endregion

		public CompositeSearchViewModel SearchByAddressViewModel => _searchByAddressViewModel;

		public virtual Organization Organisation
		{
			get => _organisation;
			set
			{
				if(UpdateFilterField(ref _organisation, value))
				{
					CanChangeOrganisation = false;
				}
			}
		}

		public bool CanChangeOrganisation { get; private set; } = true;

		public virtual PaymentFrom PaymentByCardFrom
		{
			get => _paymentByCardFrom;
			set
			{
				if(UpdateFilterField(ref _paymentByCardFrom, value))
				{
					CanChangePaymentFrom = false;
				}
			}
		}

		public bool CanChangePaymentFrom { get; private set; } = true;

		public bool PaymentsFromVisibility
		{
			get => _paymentsFromVisibility;
			set => UpdateFilterField(ref _paymentsFromVisibility, value);
		}

		public virtual OrderStatus? RestrictStatus
		{
			get => _restrictStatus;
			set
			{
				if(UpdateFilterField(ref _restrictStatus, value))
				{
					CanChangeStatus = false;
				}
			}
		}

		public bool CanChangeStatus { get; private set; } = true;

		public virtual object[] HideStatuses
		{
			get => _hideStatuses;
			set => UpdateFilterField(ref _hideStatuses, value);
		}

		public virtual OrderStatus[] AllowStatuses
		{
			get => _allowStatuses;
			set => UpdateFilterField(ref _allowStatuses, value);
		}

		public virtual PaymentType? RestrictPaymentType
		{
			get => _restrictPaymentType;
			set
			{
				if(UpdateFilterField(ref _restrictPaymentType, value))
				{
					CanChangePaymentType = false;
					PaymentsFromVisibility = _restrictPaymentType == PaymentType.PaidOnline;
					if(_restrictPaymentType != PaymentType.PaidOnline && PaymentByCardFrom != null)
					{
						PaymentByCardFrom = null;
					}
				}
			}
		}

		public bool CanChangePaymentType { get; private set; } = true;

		public virtual PaymentType[] AllowPaymentTypes
		{
			get => _allowPaymentTypes;
			set => UpdateFilterField(ref _allowPaymentTypes, value);
		}

		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public virtual Counterparty RestrictCounterparty
		{
			get => _counterparty;
			set
			{
				if(UpdateFilterField(ref _counterparty, value))
				{
					CanChangeCounterparty = false;
					OnPropertyChanged(nameof(CanChangeCounterparty));
					OnPropertyChanged(nameof(Counterparty));
					_deliveryPointJournalFilterViewModel.Counterparty = value;
					if(value == null)
					{
						DeliveryPoint = null;
						SortDeliveryDateVisibility = false;
					}
					else
					{
						CanChangeDeliveryPoint = true;
						SortDeliveryDateVisibility = true;
					}
				}
			}
		}

		public bool CanChangeCounterparty { get; private set; } = true;

		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => UpdateFilterField(ref _deliveryPoint, value);
		}

		public virtual Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}

		public virtual Employee SalesManager
		{
			get => _salesManager;
			set => UpdateFilterField(ref _salesManager, value);
		}
		
		public bool CanChangeSalesManager{
			get => _canChangeSalesManager;
			set => UpdateFilterField(ref _canChangeSalesManager, value);
		}
		
		public bool CanChangeDeliveryPoint
		{
			get => _canChangeDeliveryPoint;
			set => UpdateFilterField(ref _canChangeDeliveryPoint, value);
		}

		public virtual bool? RestrictLessThreeHours
		{
			get => _restrictLessThreeHours;
			set
			{
				if(UpdateFilterField(ref _restrictLessThreeHours, value))
				{
					CanChangeLessThreeHours = false;
				}
			}
		}

		public bool CanChangeLessThreeHours { get; private set; } = true;

		public virtual ViewTypes ViewTypes
		{
			get => _viewTypes;
			set
			{
				if(UpdateFilterField(ref _viewTypes, value))
				{
					CanChangeViewTypes = false;
				}
			}
		}

		public bool CanChangeViewTypes { get; private set; } = true;

		public OrderPaymentStatus? OrderPaymentStatus
		{
			get => _orderPaymentStatus;
			set => UpdateFilterField(ref _orderPaymentStatus, value);
		}

		public bool? IsForRetail
		{
			get => _isForRetail;
			set => UpdateFilterField(ref _isForRetail, value);
		}

		public bool? IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set => UpdateFilterField(ref _isForSalesDepartment, value);
		}

		public string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		public object EdoDocFlowStatus
		{
			get => _edoDocFlowStatus;
			set => UpdateFilterField(ref _edoDocFlowStatus, value);
		}

		#region Selfdelivery

		public virtual bool? RestrictOnlySelfDelivery
		{
			get => _restrictOnlySelfDelivery;
			set
			{
				if(UpdateFilterField(ref _restrictOnlySelfDelivery, value))
				{
					CanChangeOnlySelfDelivery = false;
					if(_restrictOnlySelfDelivery.HasValue && _restrictOnlySelfDelivery.Value && RestrictWithoutSelfDelivery.HasValue)
					{
						RestrictWithoutSelfDelivery = false;
					}
				}
			}
		}

		public bool CanChangeOnlySelfDelivery { get; private set; } = true;

		public virtual bool? RestrictWithoutSelfDelivery
		{
			get => _restrictWithoutSelfDelivery;
			set
			{
				if(UpdateFilterField(ref _restrictWithoutSelfDelivery, value))
				{
					CanChangeWithoutSelfDelivery = false;
					if(_restrictWithoutSelfDelivery.HasValue && _restrictWithoutSelfDelivery.Value && RestrictOnlySelfDelivery.HasValue)
					{
						RestrictOnlySelfDelivery = false;
					}
				}
			}
		}

		public bool CanChangeWithoutSelfDelivery { get; private set; } = true;

		public PaymentOrder? PaymentOrder
		{
			get => _paymentOrder;
			set => UpdateFilterField(ref _paymentOrder, value);
		}

		#endregion

		#region Services

		public virtual bool? RestrictHideService
		{
			get => _restrictHideService;
			set
			{
				if(UpdateFilterField(ref _restrictHideService, value))
				{
					if(_restrictHideService.HasValue && _restrictHideService.Value && RestrictOnlyService.HasValue)
					{
						RestrictOnlyService = false;
					}
				}
			}
		}

		public bool CanChangeHideService { get; private set; } = true;

		public virtual bool? RestrictOnlyService
		{
			get => _restrictOnlyService;
			set
			{
				if(UpdateFilterField(ref _restrictOnlyService, value))
				{
					CanChangeOnlyService = false;
					if(_restrictOnlyService.HasValue && _restrictOnlyService.Value && RestrictHideService.HasValue)
					{
						RestrictHideService = false;
					}
				}
			}
		}

		public bool CanChangeOnlyService { get; private set; } = true;

		#endregion

		#region Sorting

		private bool? _sortDeliveryDate;
		public virtual bool? SortDeliveryDate
		{
			get => _sortDeliveryDate;
			set => UpdateFilterField(ref _sortDeliveryDate, value);
		}

		private bool _sortDeliveryDateVisibility;
		public bool SortDeliveryDateVisibility
		{
			get => _sortDeliveryDateVisibility;
			set => SetField(ref _sortDeliveryDateVisibility, value);
		}

		#endregion

		/// <summary>
		/// Части города для отображения в фильтре
		/// </summary>
		public IEnumerable<GeoGroup> GeographicGroups => 
			_geographicGroups ?? (_geographicGroups = UoW.GetAll<GeoGroup>().Where(g => !g.IsArchived).ToList());

		private GeoGroup _geographicGroup;
		private string _counterpartyNameLike;
		private DialogViewModelBase _journal;
		private string _updDocumentNumber;

		/// <summary>
		/// Часть города
		/// </summary>
		public GeoGroup GeographicGroup
		{
			get => _geographicGroup;
			set => UpdateFilterField(ref _geographicGroup, value);
		}

		#region Date

		public bool CanChangeStartDate { get; private set; } = true;
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public bool CanChangeEndDate { get; private set; } = true;
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public virtual DateTime? RestrictStartDate
		{
			get => _restrictStartDate;
			set
			{
				if(UpdateFilterField(ref _restrictStartDate, value))
				{
					StartDate = _restrictStartDate;
					CanChangeStartDate = _restrictStartDate == null;
				}
			}
		}

		public virtual DateTime? RestrictEndDate
		{
			get => _restrictEndDate;
			set
			{
				if(UpdateFilterField(ref _restrictEndDate, value))
				{
					EndDate = _restrictEndDate;
					CanChangeEndDate = _restrictEndDate == null;
				}
			}
		}

		public virtual OrdersDateFilterType FilterDateType
		{
			get => _filterDateType;
			set => UpdateFilterField(ref _filterDateType, value);
		}

		public bool CanChangeFilterDateType { get; private set; } = true;

		public virtual OrdersDateFilterType? RestrictFilterDateType
		{
			get => _restrictFilterDateType;
			set
			{
				if(UpdateFilterField(ref _restrictFilterDateType, value))
				{
					CanChangeFilterDateType = _restrictFilterDateType == null;
				}
			}
		}

		#endregion Date

		public int? OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		public string UpdDocumentNumber
		{
			get => _updDocumentNumber;
			set
			{
				SetField(ref _updDocumentNumber, value);

				ViewTypes = string.IsNullOrWhiteSpace(_updDocumentNumber) ? ViewTypes.All : ViewTypes.Order;
			}
		}

		public int? OnlineOrderId
		{
			get => _onlineOrderId;
			set => SetField(ref _onlineOrderId, value);
		}

		public string CounterpartyPhone
		{
			get => _counterpartyPhone;
			set => SetField(ref _counterpartyPhone, value);
		}

		public string DeliveryPointPhone
		{
			get => _deliveryPointPhone;
			set => SetField(ref _deliveryPointPhone, value);
		}

		public virtual string CounterpartyNameLike
		{
			get => _counterpartyNameLike;
			set => SetField(ref _counterpartyNameLike, value);
		}

		public bool? FilterClosingDocumentDeliverySchedule
		{
			get => _filterClosingDocumentDeliverySchedule;
			set => UpdateFilterField(ref _filterClosingDocumentDeliverySchedule, value);
		}
		public override bool IsShow { get; set; } = true;

		public DialogViewModelBase Journal
		{
			get => _journal;
			set
			{
				if(_journal is null)
				{
					_journal = value;

					AuthorViewModel = new CommonEEVMBuilderFactory<OrderJournalFilterViewModel>(_journal, this, UoW, _journal.NavigationManager, _lifetimeScope)
						.ForProperty(x => x.Author)
						.UseViewModelDialog<EmployeeViewModel>()
						.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
						{
							filter.Status = EmployeeStatus.IsWorking;
						})
						.Finish();
				}
			}
		}

		private void OnSearchByAddressViewModel(object sender, EventArgs e)
		{
			Update();
		}

		public override void Dispose()
		{
			_journal = null;
			_lifetimeScope = null;
			_searchByAddressViewModel.OnSearch -= OnSearchByAddressViewModel;
			base.Dispose();
		}
	}

	public enum PaymentOrder
	{
		[Display(Name = "Оплата после отгрузки")]
		AfterShipment,
		[Display(Name = "Оплата до отгрузки")]
		BeforeShipment
	}

	public enum ViewTypes
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Заказы")]
		Order,
		[Display(Name = "Счета без отгрузки на долг")]
		OrderWSFD,
		[Display(Name = "Счета без отгрузки на предоплату")]
		OrderWSFAP,
		[Display(Name = "Счета без отгрузки на постоплату")]
		OrderWSFP
	}
	
	public enum OrdersDateFilterType
	{
		[Display(Name = "По доставке:")]
		DeliveryDate,
		[Display(Name = "По созданию:")]
		CreationDate
	}
	
}
