using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Presentation.Reports;


namespace Vodovoz.ViewModels.ReportsParameters.Logistics
{
	public class DriversToDistrictsAssignmentReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _onlyDifferentDistricts;

		public DriversToDistrictsAssignmentReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по распределению водителей на районы";
			Identifier = "Logistic.DriversToDistrictsAssignmentReport";

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

		public virtual bool OnlyDifferentDistricts
		{
			get => _onlyDifferentDistricts;
			set => SetField(ref _onlyDifferentDistricts, value);
		}


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
						{ "end_date", EndDate ?.AddDays(1).AddTicks(-1) },
						{ "only_different_districts", OnlyDifferentDistricts }
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
