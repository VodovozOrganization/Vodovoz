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
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Presentation.ViewModels.Factories;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeLastRouteListFilterFactory;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport
{
	public class LastRouteListReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<LastRouteListReportViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IIncludeExcludeLastRouteListFilterFactory _includeExcludeLastRoureListReportsFilterFactory;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private bool _isReportGenerationInProgress;
		private CancellationTokenSource _cancellationTokenSource;

		public LastRouteListReportViewModel(
			ILogger<LastRouteListReportViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IIncludeExcludeLastRouteListFilterFactory includeExcludeLastRouteListFilterFactory,
			IGuiDispatcher guiDispatcher,
			IFileDialogService fileDialogService,
			IDialogSettingsFactory dialogSettingsFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger =
				logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService =
				interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeLastRoureListReportsFilterFactory =
				includeExcludeLastRouteListFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeLastRouteListFilterFactory));
			_guiDispatcher =
				guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_dialogSettingsFactory =
				dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));

			Title = "Отчет по последнему МЛ по водителям";

			CreateFilter();

			GenerateReportCommand = new AsyncCommand(guiDispatcher, GenerateReport, () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, vm => vm.CanGenerateReport);

			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration, () => CanAbortReport);
			AbortReportGenerationCommand.CanExecuteChangedWith(this, vm => vm.CanAbortReport);

			SaveReportCommand = new DelegateCommand(SaveReport, () => CanSaveReport);
			SaveReportCommand.CanExecuteChangedWith(this, vm => vm.CanSaveReport);
		}

		private void CreateFilter()
		{
			var initIncludeFilter = new LastRouteListInitIncludeFilter
			{
				EmployeeStatusesForInclude = new[]
				{
					EmployeeStatus.IsWorking,
					EmployeeStatus.OnMaternityLeave,
					EmployeeStatus.OnCalculation
				},
				CarTypesOfUseForInclude = new[]
				{
					CarTypeOfUse.Largus,
					CarTypeOfUse.Minivan,
					CarTypeOfUse.GAZelle,
					CarTypeOfUse.Truck

				},
				CarOwnTypesForInclude = new[]
				{
					CarOwnType.Driver,
					CarOwnType.Company,
					CarOwnType.Raskat
				},
				EmployeeCategoryFilterTypeInclude = new[]
				{
					EmployeeCategoryFilterType.Driver,
					EmployeeCategoryFilterType.Forwarder
				}
			};

			FilterViewModel = _includeExcludeLastRoureListReportsFilterFactory.CreateLastReportIncludeExcludeFilter
			(
				UoW,
				initIncludeFilter
			);
		}

		private async Task GenerateReport(CancellationToken token)
		{
			if(IsReportGenerationInProgress)
			{
				return;
			}

			_logger.LogInformation("Формируем отчет");

			IsReportGenerationInProgress = true;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				await Report.GenerateRows(
					UoW,
					FilterViewModel,
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
					OnPropertyChanged(() => Report);

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
			if(IsReportGenerationInProgress)
			{
				return;
			}

			var dialogSettings = CreateDialogSettings();

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				ExportReport(saveDialogResult.Path);

				_interactiveService.ShowMessage(ImportanceLevel.Info, "Сохранение отчёта завершено.");
			}
		}
		private void ExportReport(string path)
		{
			var template = new XLTemplate(Report.TemplatePath);
			template.AddVariable(Report);
			template.Generate();
			template.SaveAs(path);
		}

		private DialogSettings CreateDialogSettings()
		{
			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(Report);

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

		public AsyncCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }

		public IncludeExludeFiltersViewModel FilterViewModel { get; private set; }

		public LastRouteListReport Report { get; private set; } = new LastRouteListReport();

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
			!IsReportGenerationInProgress
			&& Enumerable.Any(Report.Rows);

		public DateTime? FiredStartDate
		{
			get => Report.FiredStartDate;
			set => Report.FiredStartDate = value;
		}

		public DateTime? FiredEndDate
		{
			get => Report.FiredEndDate;
			set => Report.FiredEndDate = value?.AddDays(1).AddMilliseconds(-1);
		}

		public DateTime? FirstWorkDayStartDate
		{
			get => Report.FirstWorkDayStartDate;
			set => Report.FirstWorkDayStartDate = value;
		}

		public DateTime? FirstWorkDayEndDate
		{
			get => Report.FirstWorkDayEndDate;
			set => Report.FirstWorkDayEndDate = value?.AddDays(1).AddMilliseconds(-1);
		}

		public DateTime? LastRouteListStartDate
		{
			get => Report.LastRouteListStartDate;
			set => Report.LastRouteListStartDate = value;
		}

		public DateTime? LastRouteListEndDate
		{
			get => Report.LastRouteListEndDate;
			set => Report.LastRouteListEndDate = value?.AddDays(1).AddMilliseconds(-1);
		}

		public DateTime? CalculateStartDate
		{
			get => Report.CalculateStartDate;
			set => Report.CalculateStartDate = value;
		}

		public DateTime? CalculateEndDate
		{
			get => Report.CalculateEndDate;
			set => Report.CalculateEndDate = value?.AddDays(1).AddMilliseconds(-1);
		}

		public DateTime? HiredStartDate
		{
			get => Report.HiredStartDate;
			set => Report.HiredStartDate = value;
		}

		public DateTime? HiredEndDate
		{
			get => Report.HiredEndDate;
			set => Report.HiredEndDate = value?.AddDays(1).AddMilliseconds(-1);
		}


		public override void Dispose()
		{
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
