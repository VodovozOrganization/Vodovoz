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
	public class DriverWagesReportViewModel : ValidatableReportViewModelBase
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private bool _showFinesOutsidePeriod;
		private bool _showBalance;

		public DriverWagesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			Title = "Зарплата водителя";
			Identifier = "Wages.DriverWage";

			var forwarderFilter = new EmployeeFilterViewModel();
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver
			);
			_employeeJournalFactory.SetEmployeeFilterViewModel(forwarderFilter);
			DriverSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

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

		public virtual bool ShowFinesOutsidePeriod
		{
			get => _showFinesOutsidePeriod;
			set => SetField(ref _showFinesOutsidePeriod, value);
		}

		public virtual bool ShowBalance
		{
			get => _showBalance;
			set => SetField(ref _showBalance, value);
		}

		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; set; }

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var endDate = EndDate;

				if(EndDate != null)
				{
					endDate = EndDate.Value.AddHours(23).AddMinutes(59);
				}

				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate.Value },
						{ "end_date", endDate },
						{ "show_fines_outside_period", ShowFinesOutsidePeriod },
						{ "driver_id", Driver.Id },
						{ "showbalance", ShowBalance ? "1" : "0" }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать дату.", new[] { nameof(StartDate) });
			}

			if(Driver == null)
			{
				yield return new ValidationResult("Необходимо выбрать экспедитора.", new[] { nameof(Driver) });
			}
		}
	}
}
