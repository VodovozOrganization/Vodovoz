using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Sales
{
	public class SuburbWaterPriceReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;

		public SuburbWaterPriceReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по ценам пригорода";
			Identifier = "Sales.SuburbWaterPrice";

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "report_date", StartDate }
					};

				return parameters;
			}
		}

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = base.ReportInfo;
				reportInfo.ParameterDatesWithTime = false;
				return reportInfo;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать дату.", new[] { nameof(StartDate) });
			}
		}
	}
}
