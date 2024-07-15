using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public class TurnoverOfWarehouseBalancesReportViewModel : DialogViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private IncludeExludeFiltersViewModel _filterViewModel;
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private IUnitOfWork _unitOfWork;
		private CancellationTokenSource _cancellationTokenSource;
		
		private TurnoverOfWarehouseBalancesReport _report;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private DateTimeSliceType _slice;
		private bool _isReportGenerating;
		private bool _canCancelGenerateReport;
		private IEnumerable<Error> _lastGenerationErrors;
		private bool _canGenerateReport;

		public TurnoverOfWarehouseBalancesReportViewModel(
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory)
			: base(navigation)
		{
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory
				?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			Title = "Оборачиваемость складских остатков";
			_cancellationTokenSource = new CancellationTokenSource();
			ConfigureFilter();

			GenerateReportCommand = new DelegateCommand(async () => await GenerateReportAsync(), () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, x => x.CanGenerateReport);

		}

		private void ConfigureFilter()
		{
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateTurnoverOfWarehouseBalancesReportFilterViewModel(_unitOfWork);
		}

		private async Task GenerateReportAsync()
		{
			if(_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource = new CancellationTokenSource();
			}

			var reportResult = await TurnoverOfWarehouseBalancesReport.Generate(_cancellationTokenSource.Token);

			reportResult.Match(
				x => _report = x,
				errors => ShowErrors(errors));
		}

		private void ShowErrors(IEnumerable<Error> errors)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Error, string.Join("\n", errors), "Ошибка при формировании отчета!");
		}

		public TurnoverOfWarehouseBalancesReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
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

		public DateTimeSliceType Slice
		{
			get => _slice;
			set => SetField(ref _slice, value);
		}

		public bool CanGenerateReport
		{
			get => _canGenerateReport;
			set => SetField(ref _canGenerateReport, value);
		}
		public bool CanCancelGenerateReport
		{
			get => _canCancelGenerateReport;
			set => SetField(ref _canCancelGenerateReport, value);
		}

		public bool IsReportGenerating
		{
			get => _isReportGenerating;
			set => SetField(ref _isReportGenerating, value);
		}

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand ShowInfoCommand { get; }
	}
}
