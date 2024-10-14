using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Payments
{
	public class PaymentsFromAvangardReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isCustomPeriod;
		private bool _isLast3DaysPeriod;
		private bool _isYesterdayPeriod;

		public PaymentsFromAvangardReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по оплатам Авангарда";
			Identifier = "Payments.PaymentsFromAvangardReport";

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			IsYesterdayPeriod = true;
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

		public virtual bool IsCustomPeriod
		{
			get => _isCustomPeriod;
			set => SetField(ref _isCustomPeriod, value);
		}

		public virtual bool IsLast3DaysPeriod
		{
			get => _isLast3DaysPeriod;
			set
			{
				if(SetField(ref _isLast3DaysPeriod, value))
				{
					Set3daysPeriod();
				}
			}
		}

		public virtual bool IsYesterdayPeriod
		{
			get => _isYesterdayPeriod;
			set
			{
				if(SetField(ref _isYesterdayPeriod, value))
				{
					SetYesterdayPeriod();
				}
			}
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "startDate", StartDate },
						{ "endDate", EndDate?.AddHours(23).AddMinutes(59).AddSeconds(59) }
					};

				return parameters;
			}
		}

		private void Set3daysPeriod()
		{
			if(!_isLast3DaysPeriod)
			{
				return;
			}
			StartDate = DateTime.Today.AddDays(-3);
			EndDate = DateTime.Today;
		}

		private void SetYesterdayPeriod()
		{
			if(!_isYesterdayPeriod)
			{
				return;
			}
			StartDate = DateTime.Today.AddDays(-1);
			EndDate = DateTime.Today.AddDays(-1);
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
