using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.Widgets.Search;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class OnlineOrdersJournalFilterViewModel : FilterViewModelBase<OnlineOrdersJournalFilterViewModel>
	{
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private OnlineOrderPaymentStatus? _onlineOrderPaymentStatus;
		private PaymentFrom _paymentByCardFrom;
		private bool _paymentsFromVisibility;
		private Counterparty _restrictCounterparty;
		private DateTime? _restrictEndDate;
		private bool? _restrictOnlySelfDelivery;
		private bool? _restrictNeedConfirmationByCall;
		private bool? _restrictFastDelivery;
		private OnlineOrderPaymentType? _restrictPaymentType;
		private DateTime? _restrictStartDate;
		private OnlineOrderStatus? _restrictStatus;
		private Core.Domain.Clients.Source? _restrictSource;
		private bool _canChangeDeliveryPoint = true;
		private DeliveryPoint _deliveryPoint;
		private Employee _employeeWorkWith;
		private int? _orderId;
		private int? _onlineOrderId;
		private string _counterpartyPhone;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private OrdersDateFilterType? _restrictFilterDateType;
		private OrdersDateFilterType _filterDateType = OrdersDateFilterType.DeliveryDate;
		private IEnumerable<Domain.Sale.GeoGroup> _geographicGroups;
		private string _counterpartyInn;
		private readonly CompositeSearchViewModel _searchByAddressViewModel;
		private Domain.Sale.GeoGroup _geographicGroup;
		private string _counterpartyNameLike;
		private DialogViewModelBase _journal;
		private bool _sortDeliveryDateVisibility;
		private bool? _sortDeliveryDate;
		private OnlineRequestsType? _onlineRequestsType;
		private OnlinePaymentSource? _restrictOnlinePaymentSource;
		private bool _isVisibleOnlinePaymentSource;
		private bool _withoutDeliverySchedule;

		public OnlineOrdersJournalFilterViewModel(
			ILifetimeScope lifetimeScope)
		{
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_deliveryPointJournalFilterViewModel = new DeliveryPointJournalFilterViewModel();
			_searchByAddressViewModel = new CompositeSearchViewModel();
			_searchByAddressViewModel.OnSearch += OnSearchByAddressViewModel;

			StartDate = DateTime.Today.AddMonths(-1);
			EndDate = DateTime.Today.AddDays(7);
		}

		public ILifetimeScope LifetimeScope { get; private set; }
		
		#region Автосвойства
		
		public IEntityEntryViewModel DeliveryPointViewModel { get; private set; }
		public IEntityEntryViewModel EmployeeWorkWithViewModel { get; private set; }

		#endregion

		public CompositeSearchViewModel SearchByAddressViewModel => _searchByAddressViewModel;

		public PaymentFrom PaymentByCardFrom
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

		public OnlineOrderStatus? RestrictStatus
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
		
		public Core.Domain.Clients.Source? RestrictSource
		{
			get => _restrictSource;
			set
			{
				if(UpdateFilterField(ref _restrictSource, value))
				{
					CanChangeSource = false;
				}
			}
		}

		public bool CanChangeSource { get; private set; } = true;

		public OnlineRequestsType? OnlineRequestsType
		{
			get => _onlineRequestsType;
			set => UpdateFilterField(ref _onlineRequestsType, value);
		}

		public virtual OnlineOrderPaymentType? RestrictPaymentType
		{
			get => _restrictPaymentType;
			set
			{
				if(UpdateFilterField(ref _restrictPaymentType, value))
				{
					if(_restrictPaymentType is OnlineOrderPaymentType.PaidOnline)
					{
						IsVisibleOnlinePaymentSource = true;
					}
					else
					{
						IsVisibleOnlinePaymentSource = false;
						RestrictOnlinePaymentSource = null;
					}
					
					CanChangePaymentType = false;
				}
			}
		}

		public bool CanChangePaymentType { get; private set; } = true;

		public virtual OnlinePaymentSource? RestrictOnlinePaymentSource
		{
			get => _restrictOnlinePaymentSource;
			set
			{
				if(UpdateFilterField(ref _restrictOnlinePaymentSource, value))
				{
					CanChangeOnlinePaymentSource = false;
				}
			}
		}

		public bool CanChangeOnlinePaymentSource { get; private set; } = true;

		public bool IsVisibleOnlinePaymentSource
		{
			get => _isVisibleOnlinePaymentSource;
			set => SetField(ref _isVisibleOnlinePaymentSource, value);
		}

		public virtual Counterparty RestrictCounterparty
		{
			get => _restrictCounterparty;
			set
			{
				if(UpdateFilterField(ref _restrictCounterparty, value))
				{
					CanChangeCounterparty = false;
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

		public virtual Employee EmployeeWorkWith
		{
			get => _employeeWorkWith;
			set => UpdateFilterField(ref _employeeWorkWith, value);
		}

		public bool CanChangeDeliveryPoint
		{
			get => _canChangeDeliveryPoint;
			set => UpdateFilterField(ref _canChangeDeliveryPoint, value);
		}

		public OnlineOrderPaymentStatus? OnlineOrderPaymentStatus
		{
			get => _onlineOrderPaymentStatus;
			set => UpdateFilterField(ref _onlineOrderPaymentStatus, value);
		}

		public string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		public virtual bool? RestrictSelfDelivery
		{
			get => _restrictOnlySelfDelivery;
			set
			{
				if(UpdateFilterField(ref _restrictOnlySelfDelivery, value))
				{
					CanChangeOnlySelfDelivery = false;
				}
			}
		}

		public bool CanChangeOnlySelfDelivery { get; private set; } = true;
		
		public virtual bool? RestrictNeedConfirmationByCall
		{
			get => _restrictNeedConfirmationByCall;
			set
			{
				if(UpdateFilterField(ref _restrictNeedConfirmationByCall, value))
				{
					CanChangeNeedConfirmationByCall = false;
				}
			}
		}

		public bool CanChangeNeedConfirmationByCall { get; private set; } = true;
		
		public virtual bool? RestrictFastDelivery
		{
			get => _restrictFastDelivery;
			set
			{
				if(UpdateFilterField(ref _restrictFastDelivery, value))
				{
					CanChangeFastDelivery = false;
				}
			}
		}
		
		public bool WithoutDeliverySchedule
		{
			get => _withoutDeliverySchedule;
			set => UpdateFilterField(ref _withoutDeliverySchedule, value);
		}

		public bool CanChangeFastDelivery { get; private set; } = true;

		#region Sorting

		public virtual bool? SortDeliveryDate
		{
			get => _sortDeliveryDate;
			set => UpdateFilterField(ref _sortDeliveryDate, value);
		}

		public bool SortDeliveryDateVisibility
		{
			get => _sortDeliveryDateVisibility;
			set => SetField(ref _sortDeliveryDateVisibility, value);
		}

		#endregion

		/// <summary>
		/// Части города для отображения в фильтре
		/// </summary>
		public IEnumerable<Domain.Sale.GeoGroup> GeographicGroups => 
			_geographicGroups ?? (_geographicGroups = UoW.GetAll<Domain.Sale.GeoGroup>().Where(g => !g.IsArchived).ToList());

		/// <summary>
		/// Часть города
		/// </summary>
		public Domain.Sale.GeoGroup GeographicGroup
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

		public virtual string CounterpartyNameLike
		{
			get => _counterpartyNameLike;
			set => SetField(ref _counterpartyNameLike, value);
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

					var builder = new CommonEEVMBuilderFactory<OnlineOrdersJournalFilterViewModel>(
						_journal, this, UoW, _journal.NavigationManager, LifetimeScope);
					
					EmployeeWorkWithViewModel = builder.ForProperty(x => x.EmployeeWorkWith)
						.UseViewModelDialog<EmployeeViewModel>()
						.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
						{
							filter.Status = EmployeeStatus.IsWorking;
						})
						.Finish();
					
					DeliveryPointViewModel = builder.ForProperty(x => x.DeliveryPoint)
						.UseViewModelJournalAndAutocompleter<DeliveryPointByClientJournalViewModel, DeliveryPointJournalFilterViewModel>(
							_deliveryPointJournalFilterViewModel)
						.UseViewModelDialog<DeliveryPointViewModel>()
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
			LifetimeScope = null;
			_journal = null;
			_searchByAddressViewModel.OnSearch -= OnSearchByAddressViewModel;
			base.Dispose();
		}
	}
}
