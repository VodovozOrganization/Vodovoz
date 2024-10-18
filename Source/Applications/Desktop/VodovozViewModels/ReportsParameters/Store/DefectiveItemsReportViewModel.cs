﻿using Autofac;
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
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.Reports;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ViewModels.ReportsParameters.Store
{
	public class DefectiveItemsReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly ILifetimeScope _lifetimeScope;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private DefectSource? _defectSource;
		private Employee _driver;

		public DefectiveItemsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			ILifetimeScope lifetimeScope,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			Title = "Отчёт по браку";
			Identifier = "Store.DefectiveItemsReport";

			UoW = uowFactory.CreateWithoutRoot();

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			var driverFilter = _lifetimeScope.Resolve<EmployeeFilterViewModel>();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver
			);
			var employeeFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>(new TypedParameter(typeof(EmployeeFilterViewModel), driverFilter));
			EmployeeSelectorFactory = employeeFactory.CreateEmployeeAutocompleteSelectorFactory();

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;
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

		public virtual DefectSource? DefectSource
		{
			get => _defectSource;
			set => SetField(ref _defectSource, value);
		}

		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }


		public Type DefectSourceType => typeof(DefectSource);

		public Enum[] HiddenDefectSources => new Enum[] { Domain.Documents.DefectSource.None };

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate ?.ToString("yyyy-MM-dd") },
						{ "end_date", EndDate ?.ToString("yyyy-MM-dd") },
						{ "source", DefectSource == null ? "All" : DefectSource.ToString() },
						{ "driver_id", Driver?.Id ?? 0 }
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
		}
	}
}
