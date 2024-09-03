using QS.Commands;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.Reports;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ReportsParameters.Wages
{
	public class WagesOperationsReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _employee;

		public WagesOperationsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по зарплатным операциям";
			Identifier = "Employees.WagesOperations";

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			EmployeeSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
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
						{ "start_date", StartDate.Value },
						{ "end_date", EndDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59) },
						{ "employee_id", Employee.Id }
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
