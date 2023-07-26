using QS.Commands;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Employees
{
	public class EmployeeFilterViewModel : FilterViewModelBase<EmployeeFilterViewModel>, IJournalFilterViewModel
	{
		private bool _sortByPriority;
		private bool _canSortByPriority;
		private bool _canChangeStatus = true;
		private bool _canChangeCategory = true;
		private bool _hasAccessToDriverTerminal;
		private EmployeeCategory? _category;
		private EmployeeCategory? _restrictCategory;
		private EmployeeStatus? _status;
		private DriverTerminalRelation? _driverTerminalRelation;
		private WageParameterItemTypes? _restrictWageParameterItemType;
		private Subdivision _subdivision;
		private CarTypeOfUse? _driverOfCarTypeOfUse;
		private CarOwnType? _driverOfCarOwnType;
		private RegistrationType? _registrationType;
		private DateTime? _hiredDatePeriodStart;
		private DateTime? _hiredDatePeriodEnd;
		private DateTime? _firstDayOnWorkStart;
		private DateTime? _firstDayOnWorkEnd;
		private DateTime? _firedDatePeriodStart;
		private DateTime? _firedDatePeriodEnd;
		private DateTime? _settlementDateStart;
		private DateTime? _settlementDateEnd;
		private bool _isVisitingMaster;
		private bool _isDriverForOneDay;
		private bool _isChainStoreDriver;
		private bool _isRFCitizen;

		public EmployeeFilterViewModel(params EmployeeCategory[] hideEmployeeCategories)
		{
			var cashier = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.RoleCashier);
			var logistician = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			HasAccessToDriverTerminal = cashier || logistician;
			CanSortByPriority = cashier;
			HideEmployeeCategories = hideEmployeeCategories;

			CreateCommands();
		}

		#region Свойства

		public DelegateCommand UpdateRestrictions { get; private set; }
		
		public IEnumerable<EmployeeCategory> HideEmployeeCategories { get; }

		public bool CanChangeCategory
		{
			get => _canChangeCategory;
			set => SetField(ref _canChangeCategory, value);
		}

		public virtual EmployeeCategory? Category
		{
			get => _category;
			set => UpdateFilterField(ref _category, value);
		}

		public virtual EmployeeCategory? RestrictCategory
		{
			get => _restrictCategory;
			set
			{
				if(SetField(ref _restrictCategory, value))
				{
					CanChangeCategory = !RestrictCategory.HasValue;
					Category = RestrictCategory;
				}
			}
		}

		public virtual EmployeeStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}

		public bool CanChangeStatus
		{
			get => _canChangeStatus;
			set => SetField(ref _canChangeStatus, value);
		}

		public virtual WageParameterItemTypes? RestrictWageParameterItemType
		{
			get => _restrictWageParameterItemType;
			set => UpdateFilterField(ref _restrictWageParameterItemType, value);
		}

		public bool HasAccessToDriverTerminal
		{
			get => _hasAccessToDriverTerminal;
			set => SetField(ref _hasAccessToDriverTerminal, value);
		}

		public bool CanSortByPriority
		{
			get => _canSortByPriority;
			set => UpdateFilterField(ref _canSortByPriority, value);
		}

		public bool SortByPriority
		{
			get => _sortByPriority;
			set => UpdateFilterField(ref _sortByPriority, value);
		}

		public Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}

		public CarTypeOfUse? DriverOfCarTypeOfUse
		{
			get => _driverOfCarTypeOfUse;
			set => UpdateFilterField(ref _driverOfCarTypeOfUse, value);
		}

		public CarOwnType? DriverOfCarOwnType
		{
			get => _driverOfCarOwnType;
			set => UpdateFilterField(ref _driverOfCarOwnType, value);
		}

		public RegistrationType? RegistrationType
		{
			get => _registrationType;
			set => UpdateFilterField(ref _registrationType, value);
		}

		public DateTime? HiredDatePeriodStart
		{
			get => _hiredDatePeriodStart;
			set => UpdateFilterField(ref _hiredDatePeriodStart, value);
		}

		public DateTime? HiredDatePeriodEnd
		{
			get => _hiredDatePeriodEnd;
			set => UpdateFilterField(ref _hiredDatePeriodEnd, value);
		}

		public DateTime? FirstDayOnWorkStart
		{
			get => _firstDayOnWorkStart;
			set => UpdateFilterField(ref _firstDayOnWorkStart, value);
		}

		public DateTime? FirstDayOnWorkEnd
		{
			get => _firstDayOnWorkEnd;
			set => UpdateFilterField(ref _firstDayOnWorkEnd, value);
		}

		public DateTime? FiredDatePeriodStart
		{
			get => _firedDatePeriodStart;
			set => UpdateFilterField(ref _firedDatePeriodStart, value);
		}

		public DateTime? FiredDatePeriodEnd
		{
			get => _firedDatePeriodEnd;
			set => UpdateFilterField(ref _firedDatePeriodEnd, value);
		}

		public DateTime? SettlementDateStart
		{
			get => _settlementDateStart;
			set => UpdateFilterField(ref _settlementDateStart, value);
		}

		public DateTime? SettlementDateEnd
		{
			get => _settlementDateEnd;
			set => UpdateFilterField(ref _settlementDateEnd, value);
		}

		public bool IsVisitingMaster
		{
			get => _isVisitingMaster;
			set => UpdateFilterField(ref _isVisitingMaster, value);
		}

		public bool IsDriverForOneDay
		{
			get => _isDriverForOneDay;
			set => UpdateFilterField(ref _isDriverForOneDay, value);
		}

		public bool IsChainStoreDriver
		{
			get => _isChainStoreDriver;
			set => UpdateFilterField(ref _isChainStoreDriver, value);
		}

		public bool IsRFCitizen
		{
			get => _isRFCitizen;
			set => UpdateFilterField(ref _isRFCitizen, value);
		}

		public virtual DriverTerminalRelation? DriverTerminalRelation
		{
			get => _driverTerminalRelation;
			set
			{
				UpdateFilterField(ref _driverTerminalRelation, value);
				if(value == null)
				{
					Category = null;
				}
				else
				{
					Category = EmployeeCategory.driver;
				}
			}
		}

		public bool IsShow { get; set; }

		#endregion

		private void CreateCommands()
		{
			UpdateRestrictions = new DelegateCommand(
				() =>
				{
					CanChangeStatus = !SortByPriority;
					CanChangeCategory = !SortByPriority;

					if(SortByPriority)
					{
						Category = EmployeeCategory.driver;
						Status = EmployeeStatus.IsWorking;
						DriverTerminalRelation = Domain.Employees.DriverTerminalRelation.WithoutTerminal;
					}
					else
					{
						Category = RestrictCategory;
					}
				}, () => true
			);
		}
	}
}
