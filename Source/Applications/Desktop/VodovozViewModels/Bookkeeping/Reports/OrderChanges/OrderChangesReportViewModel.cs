using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Reports;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReportViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		private const string _paymentChangeTypeString = "PaymentType";

		private readonly ILogger<OrderChangesReportViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Organization> _organizationGenericRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSettings _orderSettings;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly int _monitoringPeriodAvailableInDays;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isOldMonitoring;
		private Organization _selectedOrganization;
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
			IArchiveDataSettings archiveDataSettings,
			IReportSettings reportSettings,
			IOrderSettings orderSettings,
			IDialogSettingsFactory dialogSettingsFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(reportSettings is null)
			{
				throw new ArgumentNullException(nameof(reportSettings));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_organizationGenericRepository = organizationGenericRepository ?? throw new ArgumentNullException(nameof(organizationGenericRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_monitoringPeriodAvailableInDays =
				(archiveDataSettings ?? throw new ArgumentNullException(nameof(archiveDataSettings)))
				.GetMonitoringPeriodAvailableInDays;

			Title = "Отчет по изменениям заказа при доставке";

			Organizations = GetAllOrganizations();
			SelectedOrganization = Organizations.FirstOrDefault(x => x.Id == reportSettings.GetDefaultOrderChangesOrganizationId);

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
		}

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }

		public IList<Organization> Organizations { get; }

		public IList<SelectableKeyValueNode> ChangeTypes { get; }

		public IList<SelectableKeyValueNode> IssueTypes { get; }

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

		public bool IsOldMonitoring
		{
			get => _isOldMonitoring;
			set
			{
				SetField(ref _isOldMonitoring, value);

				UpdateSelectedPeriod();
			}
		}

		[PropertyChangedAlso(
			nameof(CanGenerateReport))]
		public Organization SelectedOrganization
		{
			get => _selectedOrganization;
			set => SetField(ref _selectedOrganization, value);
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
			&& SelectedOrganization != null
			&& IsValidPeriodSelected
			&& (ChangeTypes.Any(x => x.IsSelected) || IssueTypes.Any(x => x.IsSelected));

		public bool CanAbortReport =>
			IsReportGenerationInProgress;

		public bool CanSaveReport =>
			Report != null
			&& !IsReportGenerationInProgress;

		public bool CanChangeIssueTypesSelection =>
			ChangeTypes.Any(x => x.Value == _paymentChangeTypeString && !x.IsSelected);

		public bool IsWideDateRangeWarningMessageVisible =>
			(!StartDate.HasValue && EndDate.HasValue)
			|| (StartDate.HasValue && !EndDate.HasValue && StartDate.Value < DateTime.Today.AddDays(-13))
			|| (StartDate.HasValue && EndDate.HasValue && StartDate.Value < EndDate.Value.AddDays(-13));

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
			AddChangeType("Тип оплаты заказа", _paymentChangeTypeString);
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

		public void UpdateReportGeneratingAvailabilitySettings()
		{
			OnPropertyChanged(nameof(CanChangeIssueTypesSelection));
			OnPropertyChanged(nameof(CanGenerateReport));
			OnPropertyChanged(nameof(IsWideDateRangeWarningMessageVisible));

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

			var selectedChangeTypes = ChangeTypes.Where(x => x.IsSelected);

			var selectedIssueTypes =
				CanChangeIssueTypesSelection
				? IssueTypes.Where(x => x.IsSelected)
				: Enumerable.Empty<SelectableKeyValueNode>();
			try
			{
				report = await OrderChangesReport.Create(
					UoW,
					_orderSettings,
					StartDate.Value,
					EndDate.Value,
					IsOldMonitoring,
					SelectedOrganization,
					selectedChangeTypes,
					selectedIssueTypes,
					_cancellationTokenSource.Token);
			}
			catch(OperationCanceledException ex)
			{
				var message = "Формирование отчета было прервано вручную";
				LogErrorAndShowMessageInGuiThread(ex, message);
			}
			catch(Exception ex)
			{
				var message = $"При формировании отчета возникла ошибка";

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
			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(_report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				_report.RenderTemplate(adjustColumnsToContents: false).Export(saveDialogResult.Path);
			}
		}

		private void LogErrorAndShowMessageInGuiThread(Exception ex, string message)
		{
			_logger.LogError(ex, message);

			_guiDispatcher.RunInGuiTread(() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);
			});
		}

		public bool CanClose()
		{
			if(!IsReportGenerationInProgress)
			{
				return true;
			}

			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				   "Формирование отчета в процессе.\n\n" +
				   "Чтобы закрыть вкладку необходимо либо дождаться результата формирования отчета,\n" +
				   "либо отменить его выполнение");

			return false;
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
