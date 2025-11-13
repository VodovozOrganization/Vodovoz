using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Bookkeepping.Reports.EdoControl
{
	public class EdoControlReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<EdoControlReportViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IIncludeExcludeBookkeepingReportsFilterFactory _includeExcludeBookkeeppingReportsFilterFactory;
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private int _closingDocumentDeliveryScheduleId;
		private EdoControlReport _report;
		private bool _isReportGenerationInProgress;
		private CancellationTokenSource _cancellationTokenSource;

		public EdoControlReportViewModel(
			ILogger<EdoControlReportViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IIncludeExcludeBookkeepingReportsFilterFactory includeExcludeBookkeeppingReportsFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IGuiDispatcher guiDispatcher,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(deliveryScheduleSettings is null)
			{
				throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			}

			_logger =
				logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService =
				interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeBookkeeppingReportsFilterFactory =
				includeExcludeBookkeeppingReportsFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeBookkeeppingReportsFilterFactory));
			_leftRightListViewModelFactory =
				leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			_guiDispatcher =
				guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_dialogSettingsFactory =
				dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService =
				fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			Title = "Контроль за ЭДО";

			_closingDocumentDeliveryScheduleId = deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId;

			FilterViewModel = _includeExcludeBookkeeppingReportsFilterFactory.CreateEdoControlReportIncludeExcludeFilter(UoW);

			GroupingSelectViewModel = _leftRightListViewModelFactory.CreateEdoControlReportGroupingsConstructor();

			GenerateReportCommand = new DelegateCommand(async () => await GenerateReport(), () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, vm => vm.CanGenerateReport);

			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration, () => CanAbortReport);
			AbortReportGenerationCommand.CanExecuteChangedWith(this, vm => vm.CanAbortReport);

			SaveReportCommand = new DelegateCommand(SaveReport, () => CanSaveReport);
			SaveReportCommand.CanExecuteChangedWith(this, vm => vm.CanSaveReport);
			ShowInfoCommand = new DelegateCommand(ShowInfo);
		}

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }
		public DelegateCommand ShowInfoCommand { get; }

		public IncludeExludeFiltersViewModel FilterViewModel { get; }

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel
		{
			get => _groupViewModel;
			set => SetField(ref _groupViewModel, value);
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

		public EdoControlReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		[PropertyChangedAlso(
			nameof(CanGenerateReport),
			nameof(CanAbortReport),
			nameof(CanSaveReport))]
		public bool IsReportGenerationInProgress
		{
			get => _isReportGenerationInProgress;
			set => SetField(ref _isReportGenerationInProgress, value);
		}

		public bool CanGenerateReport =>
			!IsReportGenerationInProgress;

		public bool CanAbortReport =>
			IsReportGenerationInProgress;

		public bool CanSaveReport =>
			Report != null
			&& !IsReportGenerationInProgress;

		private IEnumerable<GroupingType> SelectedGroupings =>
			GroupingSelectViewModel
			.GetRightItems()
			.Select(x => x.GroupType);

		private async Task GenerateReport()
		{
			if(IsReportGenerationInProgress)
			{
				return;
			}

			if(StartDate is null || EndDate is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Необходимо ввести полный период");

				return;
			}

			if(_cancellationTokenSource != null)
			{
				return;
			}

			_logger.LogInformation(
				"Формируем отчет контроль ЭДО.\nStartDate: {StartDate}, EndDate: {EndDate}",
				StartDate.Value,
				EndDate.Value);

			IsReportGenerationInProgress = true;
			Report = null;
			EdoControlReport report = null;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				report = await EdoControlReport.Create(
					UoW,
					StartDate.Value,
					EndDate.Value,
					_closingDocumentDeliveryScheduleId,
					FilterViewModel,
					SelectedGroupings,
					_cancellationTokenSource.Token);
			}
			catch(OperationCanceledException ex)
			{
				var message = "Формирование отчета было прервано вручную";

				LogErrorAndShowMessageInGuiThread(ex, message);
			}
			catch(Exception ex)
			{
				var message = $"При формировании отчета возникла ошибка:\n{ex.Message}";

				LogErrorAndShowMessageInGuiThread(ex, message);

			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					if(report != null)
					{
						Report = report;
					}

					IsReportGenerationInProgress = false;
				});

				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private void AbortReportGeneration()
		{
			if(!IsReportGenerationInProgress
				|| _cancellationTokenSource is null
				|| _cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}

			_cancellationTokenSource.Cancel();
		}

		private void SaveReport()
		{
			if(Report is null || IsReportGenerationInProgress)
			{
				return;
			}

			var dialogSettings = CreateDialogSettings();

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				Report.ExportToExcel(saveDialogResult.Path);
			}
		}

		private DialogSettings CreateDialogSettings()
		{
			var reportFileExtension = ".xlsx";

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = reportFileExtension,
				InitialDirectory = SpecialDirectories.Desktop,
				FileName = $"{Title} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Отчет Excel", "*" + reportFileExtension));

			return dialogSettings;
		}

		private void LogErrorAndShowMessageInGuiThread(Exception ex, string message)
		{
			_logger.LogError(ex, message);

			_guiDispatcher.RunInGuiTread(() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);
			});
		}

		private void ShowInfo()
		{
			var info =
				"1. В отчёте учитываются заказы со статусами:\r\n" +
				"   - 'Доставлен'\r\n" +
				"   - 'Выгрузка на складе'\r\n" +
				"   - 'Закрыт'\r\n" +
				"2. Допускается выбор не более 3-х группировок одновременно\r\n";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		public override void Dispose()
		{
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
