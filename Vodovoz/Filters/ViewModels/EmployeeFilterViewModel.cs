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

		private bool showFired;
		public virtual bool ShowFired {
			get => showFired;
			set {
				if(SetField(ref showFired, value, () => ShowFired)) {
					Update();
				}
			}
		}

		WageParameterTypes? restrictWageType;
		public virtual WageParameterTypes? RestrictWageType {
			get => restrictWageType;
			set => UpdateFilterField(ref restrictWageType, value);
		}

		public EmployeeFilterViewModel(ICommonServices services) : base((services ?? throw new ArgumentNullException(nameof(services))).InteractiveService)
		{
		}
	}
}