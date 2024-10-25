using QS.Commands;
using QS.DomainModel.UoW;
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
	public class DriverWagesReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private bool _showFinesOutsidePeriod;
		private bool _showBalance;

		public DriverWagesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory, validator)
		{
			var employeesFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			Title = "Зарплата водителя";
			Identifier = "Wages.DriverWage";
			
			DriverSelectorFactory = employeesFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory(true);

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
						{ "start_date", StartDate },
						{ "end_date", endDate },
						{ "show_fines_outside_period", ShowFinesOutsidePeriod },
						{ "driver_id", Driver?.Id ?? 0 },
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
				yield return new ValidationResult("Необходимо выбрать водителя.", new[] { nameof(Driver) });
			}
		}
	}
}
