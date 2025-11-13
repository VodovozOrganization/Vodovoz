using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class OrderIncorrectPricesViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _entirePeriod;

		public OrderIncorrectPricesViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по некорректным ценам";
			Identifier = "Orders.OrdersIncorrectPrices";

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;

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

		public virtual bool EntirePeriod
		{
			get => _entirePeriod;
			set => SetField(ref _entirePeriod, value);
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
				string startDate = "";
				string endDate = "";
				if(!EntirePeriod)
				{
					startDate = StartDate?.ToString("yyyy-MM-dd");
					endDate = EndDate?.ToString("yyyy-MM-dd");
				}

				var parameters = new Dictionary<string, object>
					{
						{ "dateFrom", startDate },
						{ "dateTo", endDate }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if((StartDate == null || EndDate == null) && !EntirePeriod)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate), nameof(EndDate) });
			}
		}
	}
}
