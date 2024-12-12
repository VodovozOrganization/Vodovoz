using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
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
	public class WagesOperationsReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _employee;

		public WagesOperationsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICurrentPermissionService currentPermissionService,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));

			Title = "Отчет по зарплатным операциям";
			Identifier = "Employees.WagesOperations";

			UoW = uowFactory.CreateWithoutRoot();

			var hasAccessToSalariesForLogistics = _currentPermissionService.ValidatePresetPermission("access_to_salary_reports_for_logistics");

			EmployeeFilterViewModel employeeFilter;

			if(hasAccessToSalariesForLogistics)
			{
				employeeFilter = new EmployeeFilterViewModel(EmployeeCategory.office);
				employeeFilter.SetAndRefilterAtOnce(
					x => x.Category = EmployeeCategory.driver,
					x => x.Status = EmployeeStatus.IsWorking);
			}
			else
			{
				employeeFilter = new EmployeeFilterViewModel();
				employeeFilter.SetAndRefilterAtOnce(x => x.Status = EmployeeStatus.IsWorking);
			}

			employeeFilter.HidenByDefault = true;
			employeeJournalFactory.SetEmployeeFilterViewModel(employeeFilter);

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			EmployeeSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
		}

		public DelegateCommand GenerateReportCommand;
		private readonly ICurrentPermissionService _currentPermissionService;

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

		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; private set; }

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = base.ReportInfo;
				reportInfo.UseUserVariables = true;
				return reportInfo;
			}
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate?.AddHours(23).AddMinutes(59).AddSeconds(59) },
						{ "employee_id", Employee?.Id }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null || EndDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate) });
			}

			if(Employee == null)
			{
				yield return new ValidationResult("Необходимо выбрать сотрудника.", new[] { nameof(Employee) });
			}
		}
	}
}
