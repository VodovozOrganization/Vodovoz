using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class NotDeliveredOrdersReportViewModel : ReportParametersViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;

		public NotDeliveredOrdersReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по недовозам";
			Identifier = "Orders.NotDeliveredOrders";

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;

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
						{ "startDate", StartDate },
						{ "endDate", EndDate }
					};

				return parameters;
			}
		}
	}
}
