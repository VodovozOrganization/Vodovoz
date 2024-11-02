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
	public class IncomeBalanceReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private IncomeReportType _reportType;

		public IncomeBalanceReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по приходу по кассе";

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;
			ReportType = IncomeReportType.Сommon;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand { get; }

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

		public Type IncomeReportTypeType => typeof(IncomeReportType);

		public virtual IncomeReportType ReportType
		{
			get => _reportType;
			set => SetField(ref _reportType, value);
		}

		public override string Identifier
		{
			get
			{
				switch(ReportType)
				{
					case IncomeReportType.ByRouteList:
						return "Sales.IncomeBalanceByMl";
					case IncomeReportType.BySelfDelivery:
						return "Sales.IncomeBalanceBySelfDelivery";
					default:
						return "Sales.CommonIncomeBalance";
				}
			}
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
						{ "StartDate", StartDate?.ToString("yyyy-MM-dd") },
						{ "EndDate", EndDate?.ToString("yyyy-MM-dd") }
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
