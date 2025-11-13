using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public class ExtraBottleReportViewModel : ReportParametersViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;

		public ExtraBottleReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по пересданной таре водителями";
			Identifier = "Bottles.ExtraBottlesReport";

			_startDate = DateTime.Now.AddMonths(-2);
			_endDate = DateTime.Now;

			GenerateReportCommand = new DelegateCommand(LoadReport);
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
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate }
					};

				return parameters;
			}
		}
	}
}
