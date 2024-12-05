using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<OrderChangesReportViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Organization> _organizationGenericRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;
		private readonly int _monitoringPeriodAvailableInDays;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isOldMonitoring;
		private Organization _organization;
		private bool _isReportGenerationInProgress;
		private OrderChangesReport _report;
		private CancellationTokenSource _cancellationTokenSource;

		public OrderChangesReportViewModel(
			ILogger<OrderChangesReportViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IGenericRepository<Organization> organizationGenericRepository,
			IGuiDispatcher guiDispatcher,
			IFileDialogService fileDialogService,
			IArchiveDataSettings archiveDataSettings)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_organizationGenericRepository = organizationGenericRepository ?? throw new ArgumentNullException(nameof(organizationGenericRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			_monitoringPeriodAvailableInDays =
				(archiveDataSettings ?? throw new ArgumentNullException(nameof(archiveDataSettings)))
				.GetMonitoringPeriodAvailableInDays;

			Organizations = GetAllOrganizations();

			UpdateSelectedPeriod();

			ChangeTypes = new List<SelectableKeyValueNode>();
			FillChangeTypes();

			IssueTypes = new List<SelectableKeyValueNode>();
			FillIssueTypes();

			GenerateReportCommand = new DelegateCommand(async () => await GenerateReport(), () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, vm => vm.CanGenerateReport);

			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration, () => CanAbortReport);
			AbortReportGenerationCommand.CanExecuteChangedWith(this, vm => vm.CanAbortReport);

			SaveReportCommand = new DelegateCommand(SaveReport, () => CanSaveReport);
			SaveReportCommand.CanExecuteChangedWith(this, vm => vm.CanSaveReport);

			UpdateSelectedPeriodCommand = new DelegateCommand(UpdateSelectedPeriod);
		}

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }
		public DelegateCommand UpdateSelectedPeriodCommand { get; }

		public IList<Organization> Organizations { get; }

		public IList<SelectableKeyValueNode> ChangeTypes { get; }

		public IList<SelectableKeyValueNode> IssueTypes { get; }

		public DateTime? StartDate
		{
			get => _startDate;
			set
			{
				SetField(ref _startDate, value);

				UpdateReportGeneratingAvailabilitySettings();
			}
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set
			{
				SetField(ref _endDate, value);

				UpdateReportGeneratingAvailabilitySettings();
			}
		}

		public bool IsOldMonitoring
		{
			get => _isOldMonitoring;
			set
			{
				SetField(ref _isOldMonitoring, value);

				UpdateReportGeneratingAvailabilitySettings();
			}
		}

		[PropertyChangedAlso(
			nameof(CanGenerateReport))]
		public Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
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

		[PropertyChangedAlso(
			nameof(CanSaveReport))]
		public OrderChangesReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public bool IsValidOldMonitoringPeriod =>
			StartDate.HasValue
			&& EndDate.HasValue
			&& StartDate < DateTime.Today.AddDays(_monitoringPeriodAvailableInDays + 1)
			&& EndDate < DateTime.Today.AddDays(-_monitoringPeriodAvailableInDays + 1);

		public bool IsValidNewPeriod =>
			StartDate.HasValue
			&& StartDate >= DateTime.Today.AddDays(-_monitoringPeriodAvailableInDays + 1);

		[PropertyChangedAlso(
			nameof(CanGenerateReport))]
		public bool IsValidPeriodSelected =>
			IsOldMonitoring
			? IsValidOldMonitoringPeriod
			: IsValidNewPeriod;

		public bool CanGenerateReport =>
			!IsReportGenerationInProgress
			&& Organization != null
			&& IsValidPeriodSelected
			&& (ChangeTypes.Any(x => x.IsSelected) || IssueTypes.Any(x => x.IsSelected));

		public bool CanAbortReport =>
			IsReportGenerationInProgress;

		public bool CanSaveReport =>
			Report != null
			&& !IsReportGenerationInProgress;

		public bool CanChangeIssueTypesSelection =>
			ChangeTypes.Any(x => x.Value == "PaymentType" && !x.IsSelected);

		private IList<Organization> GetAllOrganizations()
		{
			var organizations = _organizationGenericRepository.Get(UoW).ToList();

			return organizations;
		}

		private void UpdateSelectedPeriod()
		{
			var selectedDate =
				IsOldMonitoring
				? DateTime.Today.AddDays(-_monitoringPeriodAvailableInDays)
				: DateTime.Today.AddDays(-1);

			StartDate = selectedDate;
			EndDate = selectedDate;
		}

		private void FillChangeTypes()
		{
			if(ChangeTypes is null)
			{
				throw new InvalidOperationException($"Список типов изменений не инициализирован");
			}

			AddChangeType("Фактическое кол-во товара", "ActualCount");
			AddChangeType("Цена товара", "Price");
			AddChangeType("Добавление/Удаление товаров", "OrderItemsCount");
			AddChangeType("Тип оплаты заказа", "PaymentType");
		}

		private void FillIssueTypes()
		{
			if(IssueTypes is null)
			{
				throw new InvalidOperationException($"Список типов проблем не инициализирован");
			}

			AddIssueType("Проблемы с смс", "SmsIssues");
			AddIssueType("Проблемы с qr", "QrIssues");
			AddIssueType("Проблемы с терминалами", "TerminalIssues");
			AddIssueType("Проблемы менеджеров", "ManagersIssues");
		}

		private void AddChangeType(string key, string value)
		{
			var changeType = new SelectableKeyValueNode(key, value);

			changeType.PropertyChanged += OnChangeTypeSelectionPropertyChanged;

			ChangeTypes.Add(changeType);
		}

		private void AddIssueType(string key, string value)
		{
			var issueType = new SelectableKeyValueNode(key, value);

			issueType.PropertyChanged += OnIssueTypeSelectionPropertyChanged;

			IssueTypes.Add(issueType);
		}

		private void OnChangeTypeSelectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateReportGeneratingAvailabilitySettings();
		}

		private void OnIssueTypeSelectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateReportGeneratingAvailabilitySettings();
		}

		private void UpdateReportGeneratingAvailabilitySettings()
		{
			OnPropertyChanged(nameof(CanChangeIssueTypesSelection));
			OnPropertyChanged(nameof(CanGenerateReport));

			if(!IsValidPeriodSelected)
			{
				var warningMessage =
					IsOldMonitoring
					? $"Можно выбирать период только раньше {_monitoringPeriodAvailableInDays} дней"
					: $"Можно выбирать только последние {_monitoringPeriodAvailableInDays} дней";

				_interactiveService.ShowMessage(ImportanceLevel.Warning, warningMessage);
			}
		}

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
				"Формируем отчет по изменениям заказа при доставке.\nStartDate: {StartDate}, EndDate: {EndDate}",
				StartDate.Value,
				EndDate.Value);

			IsReportGenerationInProgress = true;
			Report = null;
			OrderChangesReport report = null;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				report = await OrderChangesReport.Create();
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

		public override void Dispose()
		{
			foreach(var changeType in ChangeTypes)
			{
				changeType.PropertyChanged -= OnChangeTypeSelectionPropertyChanged;
			}

			foreach(var issueType in IssueTypes)
			{
				issueType.PropertyChanged -= OnIssueTypeSelectionPropertyChanged;
			}

			base.Dispose();
		}
	}
}
