using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters;

namespace Vodovoz.JournalFilters
{
	public class EmployeeRepresentationFilterViewModel : RepresentationFilterViewModelBase<EmployeeRepresentationFilterViewModel>
	{
		private EmployeeCategory? _category;
		private EmployeeCategory? _restrictCategory;
		private EmployeeStatus? _status;
		private bool _canChangeStatus = true;
		WageParameterItemTypes? _restrictWageParameterItemType;
		
		public virtual EmployeeCategory? Category
		{
			get => _category;
			set => UpdateFilterField(ref _category, value);
		}

		[PropertyChangedAlso(nameof(IsCategoryNotRestricted))]
		public virtual EmployeeCategory? RestrictCategory
		{
			get => _restrictCategory;
			set 
			{
				if(SetField(ref _restrictCategory, value))
				{
					Category = RestrictCategory;
					Update();
				}
			}
		}

		public bool IsCategoryNotRestricted => !RestrictCategory.HasValue;

		public virtual EmployeeStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}

		public bool CanChangeStatus
		{
			get => _canChangeStatus; 
			set => UpdateFilterField(ref _canChangeStatus, value); 
		}

		public virtual WageParameterItemTypes? RestrictWageParameterItemType
		{
			get => _restrictWageParameterItemType;
			set => UpdateFilterField(ref _restrictWageParameterItemType, value);
		}
	}
}
