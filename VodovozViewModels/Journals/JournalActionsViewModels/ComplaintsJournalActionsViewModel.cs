using System;
using QS.Commands;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using Vodovoz.TempAdapters;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class ComplaintsJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private readonly IReportViewOpener _reportViewOpener;
		private DelegateCommand _openReportCommand;
		private Func<ReportInfo> _getReportInfoFunc;

		public ComplaintsJournalActionsViewModel(
			IInteractiveService interactiveService,
			IReportViewOpener reportViewOpener) : base(interactiveService)
		{
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
		}

		public DelegateCommand OpenReportCommand => _openReportCommand ?? (_openReportCommand = new DelegateCommand(
				() =>
				{
					_reportViewOpener.OpenReportInSlaveTab(journalTab, _getReportInfoFunc?.Invoke());
				},
				() => _getReportInfoFunc != null
			)
		);

		public void SetReportInfoFunc(Func<ReportInfo> reportInfoFunc)
		{
			_getReportInfoFunc = reportInfoFunc;
		}
	}
}