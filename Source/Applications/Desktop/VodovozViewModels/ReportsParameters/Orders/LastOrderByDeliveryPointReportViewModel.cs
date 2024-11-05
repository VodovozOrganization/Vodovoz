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
	public class LastOrderByDeliveryPointReportViewModel : ValidatableReportViewModelBase
	{
		private DateTime? _startDate;
		private bool _sanitary;
		private int _bottleDept;

		public LastOrderByDeliveryPointReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			Title = "Отчет по последнему заказу";

			_startDate = DateTime.Today;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual bool Sanitary
		{
			get => _sanitary;
			set => SetField(ref _sanitary, value);
		}

		public virtual int BottleDept
		{
			get => _bottleDept;
			set => SetField(ref _bottleDept, value);
		}


		public override string Identifier => Sanitary ? "Orders.SanitaryReport" : "Orders.OrdersByDeliveryPoint";

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				bool isSortByBottles;
				if(BottleDept == 0)
				{
					isSortByBottles = false;
				}
				else
				{
					isSortByBottles = true;
				}

				var parameters = new Dictionary<string, object>
					{
						{ "date", StartDate },
						{ "bottles_count", BottleDept},
						{ "is_sort_bottles", isSortByBottles }
					};

				return parameters;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate) });
			}
		}
	}
}
