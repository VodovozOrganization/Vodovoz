using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Client
{
	public class ZeroDebtClientReportViewModel : ReportParametersViewModelBase
	{
		private DateTime _startDate;
		private DateTime _endDate;

		public ZeroDebtClientReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по нулевому долгу клиента";
			Identifier = "Client.ZeroDebtClient";

			GenerateReportCommand = new DelegateCommand(LoadReport);

			_startDate = DateTime.Now.Date;
			_endDate = DateTime.Now.Date;
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime EndDate
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

