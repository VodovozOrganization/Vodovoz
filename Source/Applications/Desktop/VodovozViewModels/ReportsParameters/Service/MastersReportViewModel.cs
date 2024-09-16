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

namespace Vodovoz.ViewModels.ReportsParameters.Service
{
	public class MastersReportViewModel : ValidatableUoWReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;

		public MastersReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчет по выездным мастерам";
			Identifier = "ServiceCenter.MastersReport";

			UoW = uowFactory.CreateWithoutRoot();

			var driversFilter = new EmployeeFilterViewModel();
			driversFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver
			);
			_employeeJournalFactory.SetEmployeeFilterViewModel(driversFilter);
			DriverSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IUnitOfWorkFactory _uowFactory;

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

		public IEntityAutocompleteSelectorFactory DriverSelectorFactory  { get; }


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
						{ "end_date", EndDate },
						{ "driver_id", Driver.Id }
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

			if(Driver == null)
			{
				yield return new ValidationResult("Необходимо выбрать водителя.", new[] { nameof(Driver) });
			}

		}
	}
}
