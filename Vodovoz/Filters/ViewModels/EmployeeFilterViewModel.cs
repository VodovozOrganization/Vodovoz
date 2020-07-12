using System;
using QS.DomainModel.Entity;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Filters.ViewModels
{
	public class EmployeeFilterViewModel : RepresentationFilterViewModelBase<EmployeeFilterViewModel>
	{
		private EmployeeCategory? category;
		public virtual EmployeeCategory? Category {
			get => category;
			set {
				if(SetField(ref category, value, () => Category)) {
					Update();
				}
			}
		}

		private EmployeeCategory? restrictCategory;
		[PropertyChangedAlso(nameof(IsCategoryNotRestricted))]
		public virtual EmployeeCategory? RestrictCategory {
			get => restrictCategory;
			set {
				if(SetField(ref restrictCategory, value, () => RestrictCategory)) {
					Category = RestrictCategory;
					Update();
				}
			}
		}

		public bool IsCategoryNotRestricted => !RestrictCategory.HasValue;

		EmployeeStatus? status;
		public virtual EmployeeStatus? Status {
			get => status;
			set => UpdateFilterField(ref status, value, () => Status);
		}

		private bool canChangeStatus = true;
		public bool CanChangeStatus {
			get => canChangeStatus; 
			set => UpdateFilterField(ref canChangeStatus, value, () => CanChangeStatus); 
		}

		private DateTime? weekDay;
		public virtual DateTime? WeekDay {
			get => weekDay;
			set {
				if(SetField(ref weekDay, value))
					Update();
			}
		}

		private TimeSpan? drvStartTime;
		public virtual TimeSpan? DrvStartTime {
			get => drvStartTime;
			set {
				if(SetField(ref drvStartTime, value))
					Update();
			}
		}

		private TimeSpan? drvEndTime;
		public virtual TimeSpan? DrvEndTime {
			get => drvEndTime;
			set {
				if(SetField(ref drvEndTime, value))
					Update();
			}
		}

		WageParameterItemTypes? restrictWageParameterItemType;
		public virtual WageParameterItemTypes? RestrictWageParameterItemType {
			get => restrictWageParameterItemType;
			set => UpdateFilterField(ref restrictWageParameterItemType, value);
		}
	}
}