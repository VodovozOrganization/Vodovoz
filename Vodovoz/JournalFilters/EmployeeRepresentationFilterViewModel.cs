using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters;

namespace Vodovoz.JournalFilters
{
	public class EmployeeRepresentationFilterViewModel : RepresentationFilterViewModelBase<EmployeeRepresentationFilterViewModel>
	{
		private bool _sortByPriority;
		private bool _canSortByPriotity;
		private bool _canChangeStatus = true;
		private bool _canChangeCategory = true;
		private bool _hasAccessToDriverTerminal;
		private EmployeeCategory? _category;
		private EmployeeCategory? _restrictCategory;
		private EmployeeStatus? _status;
		private DriverTerminalRelation? _driverTerminalRelation;
		private WageParameterItemTypes? _restrictWageParameterItemType;

		#region Свойства
		public DelegateCommand UpdateRestrictions { get; private set; }

		public bool CanChangeCategory
		{
			get => _canChangeCategory;
			set => UpdateFilterField(ref _canChangeCategory, value, () => CanChangeCategory);
		}

		public virtual EmployeeCategory? Category
		{
			get => _category;
			set {
				if(SetField(ref _category, value, () => Category)) {
					Update();
				}
			}
		}

		public virtual EmployeeCategory? RestrictCategory
		{
			get => _restrictCategory;
			set {
				if(SetField(ref _restrictCategory, value, () => RestrictCategory))
				{
					CanChangeCategory = !RestrictCategory.HasValue;
					Category = RestrictCategory;
					Update();
				}
			}
		}

		public virtual EmployeeStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value, () => Status);
		}

		public bool CanChangeStatus
		{
			get => _canChangeStatus;
			set => UpdateFilterField(ref _canChangeStatus, value, () => CanChangeStatus);
		}

		public virtual WageParameterItemTypes? RestrictWageParameterItemType
		{
			get => _restrictWageParameterItemType;
			set => UpdateFilterField(ref _restrictWageParameterItemType, value);
		}

		public bool HasAccessToDriverTerminal
		{
			get => _hasAccessToDriverTerminal;
			set => UpdateFilterField(ref _hasAccessToDriverTerminal, value, () => HasAccessToDriverTerminal);
		}

		public bool CanSortByPriority
		{
			get => _canSortByPriotity;
			set => UpdateFilterField(ref _canSortByPriotity, value, () => CanSortByPriority);
		}

		public bool SortByPriority
		{
			get => _sortByPriority;
			set => UpdateFilterField(ref _sortByPriority, value, () => SortByPriority);
		}

		public virtual DriverTerminalRelation? DriverTerminalRelation
		{
			get => _driverTerminalRelation;
			set
			{
				UpdateFilterField(ref _driverTerminalRelation, value, () => DriverTerminalRelation);
				if(value != null)
				{
					Category = EmployeeCategory.driver;
				}
				else
				{
					Category = null;
				}
			}
		}

		#endregion

		public EmployeeRepresentationFilterViewModel()
		{
			var cashier = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier");
			var logistician = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			HasAccessToDriverTerminal = cashier || logistician;
			CanSortByPriority = cashier;

			CreateCommands();
		}

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
