﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Employees;

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
		private Counterparty _restrictCounterparty;
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
		private DeliveryPoint _deliveryPoint;
		private Employee _author;
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
		private bool _excludeClosingDocumentDeliverySchedule;

		#endregion

		public OrderJournalFilterViewModel(
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory)
		{
			_deliveryPointJournalFilterViewModel = new DeliveryPointJournalFilterViewModel();
			deliveryPointJournalFactory?.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilterViewModel);
			DeliveryPointSelectorFactory = deliveryPointJournalFactory?.CreateDeliveryPointByClientAutocompleteSelectorFactory()
										   ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));

			CounterpartySelectorFactory = counterpartyJournalFactory?.CreateCounterpartyAutocompleteSelectorFactory()
										  ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));

			AuthorSelectorFactory = employeeJournalFactory?.CreateEmployeeAutocompleteSelectorFactory()
									?? throw new ArgumentNullException(nameof(employeeJournalFactory));
		}

		#region Автосвойства
		public int[] IncludeDistrictsIds { get; set; }
		public int[] ExceptIds { get; set; }
		public IEnumerable<Organization> Organisations => _organisations ?? (_organisations = UoW.GetAll<Organization>().ToList());
		public IEnumerable<PaymentFrom> PaymentsFrom => _paymentsFrom ?? (_paymentsFrom = UoW.GetAll<PaymentFrom>().ToList());
		public virtual IEntityAutocompleteSelectorFactory DeliveryPointSelectorFactory { get; }
		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public virtual IEntityAutocompleteSelectorFactory AuthorSelectorFactory { get; }

		#endregion

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
					PaymentsFromVisibility = _restrictPaymentType == PaymentType.ByCard;
					if(_restrictPaymentType != PaymentType.ByCard && PaymentByCardFrom != null)
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

		public virtual Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
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
			set => UpdateFilterField(ref _viewTypes, value);
		}

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
		public IEnumerable<GeoGroup> GeographicGroups => _geographicGroups ?? (_geographicGroups = UoW.GetAll<GeoGroup>().ToList());

		private GeoGroup _geographicGroup;
		private string _counterpartyNameLike;
		private string _deliveryPointAddressLike;

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

		public virtual string DeliveryPointAddressLike
		{
			get => _deliveryPointAddressLike;
			set => SetField(ref _deliveryPointAddressLike, value);
		}

		public bool ExcludeClosingDocumentDeliverySchedule
		{
			get => _excludeClosingDocumentDeliverySchedule;
			set => UpdateFilterField(ref _excludeClosingDocumentDeliverySchedule, value);
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
