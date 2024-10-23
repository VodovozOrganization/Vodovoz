using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Pacs.Reports
{
	public class PacsMissingCallsReportViewModel : ReportParametersViewModelBase
	{
		private DateTime _dateFrom;
		private DateTime _dateTo;

		public PacsMissingCallsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по пропущенным звонкам";
			Identifier = "Pacs.PacsMissedCalls";

			DateFrom = DateTime.Now.AddDays(-1);
			DateTo = DateTime.Now;

			GenerateReportCommand = new DelegateCommand(LoadReport);
		}

		public DelegateCommand GenerateReportCommand { get; }

		public virtual DateTime DateFrom
		{
			get => _dateFrom;
			set => SetField(ref _dateFrom, value);
		}

		public virtual DateTime DateTo
		{
			get => _dateTo;
			set => SetField(ref _dateTo, value);
		}

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "date_from", DateFrom.ToString("yyyy-MM-dd HH:mm:ss") },
			{ "date_to", DateTo.Date.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss") }
		};

		public string Info => "Отчет представляет звонки на которые не ответили операторы. " +
			"К каждому звонку подбирает информацию об операторах, которые могли в этот момент ответить на звонок.";

	}
}
