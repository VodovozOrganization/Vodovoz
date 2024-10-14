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
	public class AnalyticsForUndeliveryReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;

		public AnalyticsForUndeliveryReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Аналитика по недовозам";
			Identifier = "Logistic.AnalyticsForUndelivery";

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

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				int[] geoparts = { 1, 2, 3 };
				var parameters = new Dictionary<string, object>
					{
						{ "first_date", StartDate },
						{ "second_date", EndDate },
						{ "title_date", GetTitleDate() },
						{ "geoparts", geoparts }
					};

				return parameters;
			}
		}

		public string GetTitleDate()
		{
			var titleDate = StartDate.Value.ToShortDateString();

			if(EndDate != null && EndDate.Value != StartDate.Value)
			{
				titleDate = titleDate + " и на " + EndDate.Value.ToShortDateString();
			}

			return titleDate;
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
