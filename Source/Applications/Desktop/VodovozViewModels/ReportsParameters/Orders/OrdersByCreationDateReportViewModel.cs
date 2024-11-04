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
	public class OrdersByCreationDateReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;

		public OrdersByCreationDateReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по дате создания заказа";
			Identifier = "Bottles.OrdersByCreationDate";

			_startDate = DateTime.Today;

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
						{ "date", StartDate?.ToString("yyyy-MM-dd") }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null )
			{
				yield return new ValidationResult("Необходимо выбрать дату.", new[] { nameof(StartDate) });
			}
		}
	}
}
