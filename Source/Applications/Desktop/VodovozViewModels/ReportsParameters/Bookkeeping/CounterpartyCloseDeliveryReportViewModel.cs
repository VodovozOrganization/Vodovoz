using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Bookkeeping
{
	public class CounterpartyCloseDeliveryReportViewModel : ReportParametersViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;

		public CounterpartyCloseDeliveryReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет закрытых отгрузок";
			Identifier = "Bookkeeping.CloseCounterpartyDelivery";

			GenerateReportCommand = new DelegateCommand(LoadReport);

			_startDate = DateTime.Now.AddMonths(-1);
			_endDate = DateTime.Now;
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
