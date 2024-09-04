using QS.Commands;
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
	public class EmployeesFinesViewModel : ValidatableReportViewModelBase
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private bool _categoryDriver;
		private bool _categoryForwarder;
		private bool _categoryOffice;

		public EmployeesFinesViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			Title = "Штрафы сотрудников";
			Identifier = "Employees.Fines";

			var _employeeFilter = new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking };
			_employeeJournalFactory.SetEmployeeFilterViewModel(_employeeFilter);
			DriverSelectorFactory = _employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();

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
			set => SetField(ref _categoryDriver, value);
		}

		public virtual bool CategoryForwarder
		{
			get => _categoryForwarder;
			set => SetField(ref _categoryForwarder, value);
		}

		public virtual bool CategoryOffice
		{
			get => _categoryOffice;
			set => SetField(ref _categoryOffice, value);
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
					parameters.Add("driverName", " " + Driver.FullName);
				}
				else
				{
					parameters.Add("drivers", -1);
				}
				if(StartDate != null && EndDate != null)
				{
					parameters.Add("startDate", StartDate.Value);
					parameters.Add("endDate", EndDate.Value);
				}
				else
				{
					parameters.Add("startDate", 0);
					parameters.Add("endDate", 0);
				}

				parameters.Add("showbottom", false);
				parameters.Add("routelist", 0);
				parameters.Add("category", GetCategory());

				return parameters;
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
				yield return new ValidationResult("Необходимо выбрать период или водителя.", new[] { nameof(StartDate), nameof(EndDate), nameof(Driver) });
			}
			
		}
	}
}
