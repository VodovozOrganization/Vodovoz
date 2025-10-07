using ClosedXML.Report;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Services;
using Vodovoz.Settings.Logistics;
using Vodovoz.Tools;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReportParametersViewModel : DialogTabViewModelBase
	{
		private DateTime _date;
		private int _countDays;

		private CarIsNotAtLineReport _report;
		private readonly ILogger<CarIsNotAtLineReportParametersViewModel> _logger;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IGenericRepository<CarEvent> _carEventRepository;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IInteractiveService _interactiveService;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IGuiDispatcher _guiDispatcher;
		private bool _isReportGenerationInProgress;
		private CancellationTokenSource _cancellationTokenSource;

		public CarIsNotAtLineReportParametersViewModel(
			ILogger<CarIsNotAtLineReportParametersViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService,
			IGenericRepository<CarEventType> carEventTypeRepository,
			IGenericRepository<CarEvent> carEventRepository,
			IncludeExludeFilterGroupViewModel includeExludeFilterGroupViewModel,
			IUserSettingsService userSettingsService,
			IInteractiveService interactiveService,
			ICarEventSettings carEventSettings,
			INavigationManager navigation,
			IGuiDispatcher guiDispatcher)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_dialogSettingsFactory = dialogSettingsFactory
				?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_carEventRepository = carEventRepository
				?? throw new ArgumentNullException(nameof(carEventRepository));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_carEventSettings = carEventSettings
				?? throw new ArgumentNullException(nameof(carEventSettings));
			_guiDispatcher = guiDispatcher
				?? throw new ArgumentNullException(nameof(guiDispatcher));
			_userSettingsService = userSettingsService
				?? throw new ArgumentNullException(nameof(userSettingsService));

			Title = typeof(CarIsNotAtLineReport).GetClassUserFriendlyName().Nominative;

			Date = DateTime.Today;
			CountDays = 4;

			includeExludeFilterGroupViewModel.InitializeFor(UoW, carEventTypeRepository);
			includeExludeFilterGroupViewModel.RefreshFilteredElementsCommand.Execute();

			var lastIncludedElements = _userSettingsService.Settings.CarIsNotAtLineReportIncludedEventTypeIds;
			var lastExcludedElements = _userSettingsService.Settings.CarIsNotAtLineReportExcludedEventTypeIds;

			foreach(var element in includeExludeFilterGroupViewModel.Elements)
			{
				if(lastIncludedElements.Contains(int.Parse(element.Number)))
				{
					element.Include = true;
				}

				if(lastExcludedElements.Contains(int.Parse(element.Number)))
				{
					element.Exclude = true;
				}
			}

			IncludeExludeFilterGroupViewModel = includeExludeFilterGroupViewModel;

			GenerateReportCommand = new DelegateCommand(async () => await GenerateReport(), () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, vm => vm.CanGenerateReport);

			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration, () => CanAbortReport);
			AbortReportGenerationCommand.CanExecuteChangedWith(this, vm => vm.CanAbortReport);

			SaveReportCommand = new DelegateCommand(SaveReport);
			SaveReportCommand.CanExecuteChangedWith(this, vm => vm.CanSaveReport);

			SaveIncludeExcludeParametersCommand = new DelegateCommand(SaveIncludeExcludeParameters);
			ShowInfoCommand = new DelegateCommand(ShowInfo);
		}

		public IncludeExludeFilterGroupViewModel IncludeExludeFilterGroupViewModel { get; }

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }
		public DelegateCommand SaveIncludeExcludeParametersCommand { get; }
		public DelegateCommand ShowInfoCommand { get; }

		public DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		public int CountDays
		{
			get => _countDays;
			set => SetField(ref _countDays, value);
		}

		public CarIsNotAtLineReport Report
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

		private async Task GenerateReport()
		{
			if(IsReportGenerationInProgress || _cancellationTokenSource != null)
			{
				return;
			}

			_logger.LogInformation(
				"Формируем отчет по простою.Дата: {Date}, Период: {Period}",
				Date,
				CountDays);

			IsReportGenerationInProgress = true;
			Report = null;
			CarIsNotAtLineReport report = null;

			var reportName = typeof(CarIsNotAtLineReport).GetClassUserFriendlyName().Nominative;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				var reportResult = await CarIsNotAtLineReport.Generate(
					UoW,
					_carEventRepository,
					Date,
					CountDays,
					IncludeExludeFilterGroupViewModel.IncludedElements.Select(e => (int.Parse(e.Number), e.Title)),
					IncludeExludeFilterGroupViewModel.ExcludedElements.Select(e => (int.Parse(e.Number), e.Title)),
					_carEventSettings.CarsExcludedFromReportsIds,
					_carEventSettings.CarTransferEventTypeId,
					_carEventSettings.CarReceptionEventTypeId,
					_cancellationTokenSource.Token);

				reportResult.Match(
					r => report = r,
					errors =>
					{
						_logger.LogError(
							"При формировании отчета {ReportName} возникли ошибки:\n{Errors}",
							reportName,
							string.Join("\n",
							errors));

						ShowMessageInGuiThread("Отчет не может быть сформирован, так как возникли ошибки:\n" + string.Join("\n", errors));
					});
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

					SaveIncludeExcludeParametersCommand.Execute();
				});

				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private void LogErrorAndShowMessageInGuiThread(Exception ex, string message)
		{
			LogError(ex, message);
			ShowMessageInGuiThread(message);
		}

		private void LogError(Exception ex, string message = null)
		{
			if(string.IsNullOrWhiteSpace(message))
			{
				message = $"При формировании отчета возникла ошибка:\n{ex.Message}";
			}

			_logger.LogError(ex, message);
		}

		private void ShowMessageInGuiThread(string message)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);
			});
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
			if(Report is null)
			{
				return;
			}

			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(Report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				PostProcess(Report)
					.Export(saveDialogResult.Path);
			}
		}

		private void ShowInfo()
		{
			var info =
				"Условные обозначения отчёта: \"К\" - ТС компании, \"В\" - ТС водителя, \"Р\" - ТС в раскате\r\n"+
				"Пояснение к столбикам: \"П\" - принадлежность\r\n" +
				"1. В отчет попадают авто приналежности:\r\n" +
				"   - 'ТС компании'\r\n" +
				"   - 'ТС в раскате'\r\n" +
				"2. В отчет не попадают авто типа:\r\n" +
				"   - 'Фура'\r\n" +
				"   - 'Погрузчик'\r\n" +
				"3. При формировании отчета запрашиваются данные по последнему МЛ для каждого авто,\r\n" +
				"подходящего по типу и принадлежности на выбранную дату (включительно).\r\n" +
				"Если в течение установленного количества дней до указанной даты (включительно)\r\n" +
				"у авто отсутствует МЛ, то информация по данному авто попадает в отчет.\r\n" +
				"Если в течение установленного периода до указанной даты (включительно) найдены\r\n" +
				"события ТС, связанные с добавленными в отчет авто, то строка отчета соответствующего авто\r\n" +
				"дополняется информацией о найденных событиях.\r\n" +
				"Если событие не найдено, то описание поломки заполняется как 'Простой'";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private XLTemplate PostProcess(CarIsNotAtLineReport report)
		{
			var template = report.GetRawTemplate();

			if(!report.CarReceptionRows.Any())
			{
				template.Workbook.Worksheet(1).Rows(13, 16).Delete();
			}

			if(!report.CarTransferRows.Any())
			{
				template.Workbook.Worksheet(1).Rows(8, 11).Delete();
			}

			return template.RenderTemplate(report);
		}

		private void SaveIncludeExcludeParameters()
		{
			_userSettingsService.Settings.CarIsNotAtLineReportIncludedEventTypeIds = IncludeExludeFilterGroupViewModel.IncludedElements.Select(iee => int.Parse(iee.Number));
			_userSettingsService.Settings.CarIsNotAtLineReportExcludedEventTypeIds = IncludeExludeFilterGroupViewModel.ExcludedElements.Select(iee => int.Parse(iee.Number));
		}
	}
}
