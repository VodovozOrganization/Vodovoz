using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.Reports;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ViewModels.ReportsParameters.Wages
{
	public class EmployeesPremiumsViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly EmployeeFilterViewModel _employeeFilter;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private bool _categoryDriver;
		private bool _categoryForwarder;
		private bool _categoryOffice;

		public EmployeesPremiumsViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			EmployeeFilterViewModel employeeFilter,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory, validator)
		{
			var employeesFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeFilter = employeeFilter ?? throw new ArgumentNullException(nameof(employeeFilter));

			Title = "Премии сотрудников";
			Identifier = "Employees.Premiums";

			_employeeFilter.Status = EmployeeStatus.IsWorking;
			employeesFactory.SetEmployeeFilterViewModel(_employeeFilter);
			DriverSelectorFactory = employeesFactory.CreateEmployeeAutocompleteSelectorFactory();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public virtual bool CategoryDriver
		{
			get => _categoryDriver;
			set
			{
				SetField(ref _categoryDriver, value);
				UpdateEmployeeCategoryFilter();
			}
		}

		public virtual bool CategoryForwarder
		{
			get => _categoryForwarder;
			set
			{
				SetField(ref _categoryForwarder, value);
				UpdateEmployeeCategoryFilter();
			}
		}

		public virtual bool CategoryOffice
		{
			get => _categoryOffice;
			set
			{
				SetField(ref _categoryOffice, value);
				UpdateEmployeeCategoryFilter();
			}
		}

		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>();
				if(Driver != null)
				{
					parameters.Add("drivers", Driver.Id);
				}
				else
				{
					parameters.Add("drivers", -1);
				}
				if(StartDate != null && EndDate != null)
				{
					parameters.Add("startDate", StartDate);
					parameters.Add("endDate", EndDate);
				}
				else
				{
					parameters.Add("startDate", 0);
					parameters.Add("endDate", 0);
				}

				parameters.Add("showbottom", false);
				parameters.Add("category", GetCategory());

				return parameters;
			}
		}

		private void UpdateEmployeeCategoryFilter()
		{
			if(CategoryDriver)
			{
				_employeeFilter.RestrictCategory = EmployeeCategory.driver;
			}

			if(CategoryForwarder)
			{
				_employeeFilter.RestrictCategory = EmployeeCategory.forwarder;
			}

			if(CategoryOffice)
			{
				_employeeFilter.RestrictCategory = EmployeeCategory.office;
			}

			if(!CategoryDriver && !CategoryForwarder && !CategoryOffice)
			{
				_employeeFilter.RestrictCategory = null;
			}
		}

		private string GetCategory()
		{
			string cat = "-1";

			if(CategoryDriver)
			{
				cat = EmployeeCategory.driver.ToString();
			}
			else if(CategoryForwarder)
			{
				cat = EmployeeCategory.forwarder.ToString();
			}
			else if(CategoryOffice)
			{
				cat = EmployeeCategory.office.ToString();
			}

			return cat;
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var datePeriodSelected = StartDate != null && EndDate != null;
			var driverSelected = Driver != null;
			if (!datePeriodSelected && !driverSelected)
			{
				yield return new ValidationResult("Необходимо выбрать период или сотрудника.", new[] { nameof(StartDate), nameof(EndDate), nameof(Driver) });
			}
		}
	}
}
