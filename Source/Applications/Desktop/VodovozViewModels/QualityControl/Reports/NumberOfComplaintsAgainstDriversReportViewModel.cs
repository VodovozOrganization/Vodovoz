using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private NumberOfComplaintsAgainstDriversReport _report;
		private DateTime? _startDate;
		private DateTime? _endDate;

		public NumberOfComplaintsAgainstDriversReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			TabName = typeof(NumberOfComplaintsAgainstDriversReport).GetClassUserFriendlyName().Nominative;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public NumberOfComplaintsAgainstDriversReport Report
		{
			get => _report;
			private set => SetField(ref _report, value);
		}

		public DelegateCommand GenerateReportCommand { get; }

		private void GenerateReport()
		{
			if(StartDate is null || EndDate is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не указан период");
				return;
			}

			Report = NumberOfComplaintsAgainstDriversReport.Generate(UoW, StartDate.Value, EndDate.Value);
		}
	}
}
