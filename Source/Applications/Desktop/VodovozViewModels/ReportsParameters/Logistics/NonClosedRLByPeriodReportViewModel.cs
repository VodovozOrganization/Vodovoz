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
	public class NonClosedRLByPeriodReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private int _delay;

		public NonClosedRLByPeriodReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по незакрытым МЛ за период";
			Identifier = "Logistic.NonClosedRLByPeriodReport";

			_startDate = DateTime.Today.AddMonths(-1);
			_endDate = DateTime.Today;
			_delay = 2;

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

		public virtual int Delay
		{
			get => _delay;
			set => SetField(ref _delay, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) },
						{ "create_date", DateTime.Now },
						{ "delay", Delay }
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
