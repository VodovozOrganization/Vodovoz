using System;
using QS.DomainModel.Entity;
using QS.Services;
using Vodovoz.Domain.Employees;
namespace Vodovoz.Filters.ViewModels
{
	public class EmployeeFilterViewModel : RepresentationFilterViewModelBase<EmployeeFilterViewModel>
	{
		private EmployeeCategory? category;
		public virtual EmployeeCategory? Category {
			get => category;
			set => SetField(ref category, value, () => Category);
		}

		private EmployeeCategory? restrictCategory;
		[PropertyChangedAlso(nameof(IsCategoryNotRestricted))]
		public virtual EmployeeCategory? RestrictCategory {
			get => restrictCategory;
			set {
				if(SetField(ref restrictCategory, value, () => RestrictCategory)) {
					Category = RestrictCategory;
				}
			}
		}

		public bool IsCategoryNotRestricted => !RestrictCategory.HasValue;

		private bool showFired;
		public virtual bool ShowFired {
			get => showFired;
			set => SetField(ref showFired, value, () => ShowFired);
		}

		public EmployeeFilterViewModel(ICommonServices services) : base((services ?? throw new ArgumentNullException(nameof(services))).InteractiveService)
		{
		}
	}
}