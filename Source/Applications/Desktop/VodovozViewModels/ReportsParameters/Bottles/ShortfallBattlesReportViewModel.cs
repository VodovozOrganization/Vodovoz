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
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.Reports;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public class ShortfallBattlesReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		private DateTime? _startDate;
		private Employee _driver;
		private bool _oneDriver;
		private Drivers _driverType;
		private IEnumerable<NonReturnReason> _nonReturnReasons;
		private NonReturnReason _nonReturnReason;

		public ShortfallBattlesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			Title = "Отчет о несданных бутылях";
			Identifier = "Bottles.ShortfallBattlesReport";

			UoW = unitOfWorkFactory.CreateWithoutRoot();

			_startDate = DateTime.Today;

			DriverType = Drivers.AllDriver;

			NonReturnReasons = UoW.Session.QueryOver<NonReturnReason>().List();

			var filter = new EmployeeFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			_employeeJournalFactory.SetEmployeeFilterViewModel(filter);
			DriverSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public virtual bool OneDriver
		{
			get => _oneDriver;
			set
			{
				SetField(ref _oneDriver, value);
				if(!_oneDriver)
				{
					Driver = null;
				}
			}
		}

		public Type DriverTypeType => typeof(Drivers);

		public virtual Drivers DriverType
		{
			get => _driverType;
			set => SetField(ref _driverType, value);
		}

		public virtual IEnumerable<NonReturnReason> NonReturnReasons
		{
			get => _nonReturnReasons;
			set => SetField(ref _nonReturnReasons, value);
		}

		public virtual NonReturnReason NonReturnReason
		{
			get => _nonReturnReason;
			set => SetField(ref _nonReturnReason, value);
		}

		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = base.ReportInfo;
				reportInfo.ParameterDatesWithTime = true;
				return reportInfo;
			}
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "reason_id", NonReturnReason?.Id ?? -1 },
						{ "driver_id", Driver?.Id ?? -1 },
						{ "driver_call", (int)DriverType },
						{ "date", StartDate}
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
		}

		public enum Drivers
		{
			[Display(Name = "Все")]
			AllDriver = -1,
			[Display(Name = "Отзвон не с адреса")]
			CallFromAnywhere = 3,
			[Display(Name = "Без отзвона")]
			NoCall = 2,
			[Display(Name = "Ларгусы")]
			Largus = 1,
			[Display(Name = "Наемники")]
			Hirelings = 0
		}
	}
}
