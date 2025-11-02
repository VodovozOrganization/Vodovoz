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
	public class ForwarderWageReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _forwarder;
		private bool _showFinesOutsidePeriod;
		private bool _showBalance;

		public ForwarderWageReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory, validator)
		{
			var employeeFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			Title = "Отчет по зарплате экспедитора";
			Identifier = "Employees.ForwarderWage";
			
			ForwarderSelectorFactory = employeeFactory.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory(true);

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

		public virtual Employee Forwarder
		{
			get => _forwarder;
			set => SetField(ref _forwarder, value);
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

		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; set; }

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "show_fines_outside_period", ShowFinesOutsidePeriod },
						{ "forwarder_id", Forwarder?.Id ?? 0 },
						{ "showbalance", ShowBalance ? "1" : "0" }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null || EndDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate), nameof(EndDate) });
			}

			if(Forwarder == null)
			{
				yield return new ValidationResult("Необходимо выбрать экспедитора.", new[] { nameof(Forwarder) });
			}
		}
	}
}
