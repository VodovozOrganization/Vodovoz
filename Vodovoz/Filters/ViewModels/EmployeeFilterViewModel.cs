using System;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
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

		public override ICriterion GetFilter()
		{
			if(!ShowFired)
				Criterions.Add(Restrictions.Where<Employee>(x => !x.IsFired));
			if(Category.HasValue)
				Criterions.Add(Restrictions.Where<Employee>(x => x.Category == Category.Value));

			return base.GetFilter();
		}
	}
}
