using System;
using NHibernate.Criterion;
using Vodovoz.Domain.Employees;
using QS.DomainModel.Entity;
using Vodovoz.Infrastructure.Services;
namespace Vodovoz.Filters.ViewModels
{
	public class EmployeeFilterViewModel : FilterViewModelBase<EmployeeFilterViewModel>
	{
		private EmployeeCategory? category;
		public virtual EmployeeCategory? Category {
			get => category;
			set => SetField(ref category, value, () => Category);
		}

		private EmployeeCategory? restrictCategory;
		[PropertyChangedAlso(nameof(CategoryRestricted))]
		public virtual EmployeeCategory? RestrictCategory {
			get => restrictCategory;
			set {
				if(SetField(ref restrictCategory, value, () => RestrictCategory)) {
					Category = RestrictCategory;
				}
			}
		}

		public bool CategoryRestricted => RestrictCategory.HasValue;

		private bool showFired;
		public virtual bool ShowFired {
			get => showFired;
			set => SetField(ref showFired, value, () => ShowFired);
		}

		public EmployeeFilterViewModel(ICommonServices services) : base((services ?? throw new ArgumentNullException(nameof(services))).InteractiveService)
		{
		}

		public override ICriterion GetFilter()
		{
			ICriterion result = Restrictions.Where<Employee>(x => x.IsFired == ShowFired);

			if(Category.HasValue) {
				result = Restrictions.And(result, Restrictions.Where<Employee>(x => x.Category == Category.Value));
			}

			return result;
		}
	}
}
