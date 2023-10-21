using QS.Project.Filter;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Cash
{
	public class SalaryByEmployeeJournalFilterViewModel : FilterViewModelBase<SalaryByEmployeeJournalFilterViewModel>
	{
		private EmployeeStatus? _status;
		private Subdivision _subdivision;
		private EmployeeCategory? _category;
		private IEnumerable<Subdivision> _subdivisions;
		private decimal _minBalance;
		private bool _minBalanceFilterEnable;

		#region Свойства

		public virtual EmployeeCategory? Category
		{
			get => _category;
			set => UpdateFilterField(ref _category, value);
		}

		public virtual EmployeeStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}

		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}

		public virtual IEnumerable<Subdivision> Subdivisions
		{
			get => _subdivisions;
			set => UpdateFilterField(ref _subdivisions, value);
		}

		public decimal MinBalance
		{
			get => _minBalance;
			set => UpdateFilterField(ref _minBalance, value);
		}

		public bool MinBalanceFilterEnable
		{
			get => _minBalanceFilterEnable;
			set => UpdateFilterField(ref _minBalanceFilterEnable, value);
		}

		#endregion

		public SalaryByEmployeeJournalFilterViewModel(
			ISubdivisionRepository subdivisionRepository, EmployeeStatus? defaultStatus = null)
		{
			_subdivisions = subdivisionRepository?.GetAllDepartmentsOrderedByName(UoW)
			                ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_status = defaultStatus;
		}
	}
}
