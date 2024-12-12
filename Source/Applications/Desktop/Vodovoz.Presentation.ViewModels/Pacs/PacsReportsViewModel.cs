using QS.Commands;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels;
using System;
using Vodovoz.Presentation.ViewModels.Pacs.Reports;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsReportsViewModel : WidgetViewModelBase
	{
		private readonly INavigationManager _navigationManager;

		public PacsReportsViewModel(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));

			MissingCallsReportCommand = new DelegateCommand(OpenMissingCallsReport);
		}

		public DelegateCommand MissingCallsReportCommand { get; }

		private void OpenMissingCallsReport()
		{
			_navigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(PacsMissingCallsReportViewModel));
		}
	}
}
