using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.CompletedAddressesReport
{
	/// <summary>
	/// Вкладка отчета по выполненным адресам и районам
	/// </summary>
	public class CompletedAddressesReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<CompletedAddressesReportViewModel> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IUserRepository _userRepository;
		private readonly IUserService _userService;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;

		private readonly IncludeExcludeBoolParamsFilter _driversFilter;
		private readonly IncludeExcludeBoolParamsFilter _districtsFilter;

		private IList<(string Key, string Title)> _drivers = new List<(string Key, string Title)>();
		private IList<(string Key, string Title)> _districts = new List<(string Key, string Title)>();

		private DateTime? _startDate;
		private DateTime? _endDate;
		private CompletedAddressesReport _report;
		private bool _isReportGenerationInProgress;
		private bool _isFilterListsLoading;
		private bool _isSavedSelectionApplied;
		private CancellationTokenSource _reportCancellationTokenSource;
		private CancellationTokenSource _filterListsCancellationTokenSource;
		private Task _filterListsLoadingTask = Task.CompletedTask;

		public CompletedAddressesReportViewModel(
			ILogger<CompletedAddressesReportViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IRouteListItemRepository routeListItemRepository,
			IUserRepository userRepository,
			IUserService userService,
			IUserSettingsService userSettingsService,
			IIncludeExcludeCompletedAddressesReportFilterFactory filterFactory,
			IGuiDispatcher guiDispatcher,
			IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			if(filterFactory == null)
			{
				throw new ArgumentNullException(nameof(filterFactory));
			}

			Title = "Отчет по выполненным адресам и районам";

			FilterViewModel = filterFactory.CreateCompletedAddressesReportIncludeExcludeFilter(() => _drivers, () => _districts);

			_driversFilter = FilterViewModel.GetFilter<IncludeExcludeBoolParamsFilter>(
				IncludeExcludeCompletedAddressesReportFilterFactory.DriversFilterName);
			_districtsFilter = FilterViewModel.GetFilter<IncludeExcludeBoolParamsFilter>(
				IncludeExcludeCompletedAddressesReportFilterFactory.DistrictsFilterName);

			GenerateReportCommand = new AsyncCommand(_guiDispatcher, GenerateReport, () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, vm => vm.CanGenerateReport);

			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration, () => CanAbortReport);
			AbortReportGenerationCommand.CanExecuteChangedWith(this, vm => vm.CanAbortReport);

			ExportReportCommand = new DelegateCommand(ExportReport, () => CanExportReport);
			ExportReportCommand.CanExecuteChangedWith(this, vm => vm.CanExportReport);
		}

		/// <summary>
		/// Формирование отчета за выбранный период с учетом фильтров
		/// </summary>
		public AsyncCommand GenerateReportCommand { get; }

		/// <summary>
		/// Прерывание формирования отчета
		/// </summary>
		public DelegateCommand AbortReportGenerationCommand { get; }

		/// <summary>
		/// Выгрузка сформированного отчета в Excel
		/// </summary>
		public DelegateCommand ExportReportCommand { get; }

		/// <summary>
		/// Фильтр "Водители" и "Районы"
		/// </summary>
		public IncludeExludeFiltersViewModel FilterViewModel { get; }

		/// <summary>
		/// Начало периода (дата МЛ)
		/// </summary>
		[PropertyChangedAlso(nameof(IsPeriodSelected), nameof(CanChangeFilters), nameof(CanGenerateReport))]
		public DateTime? StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value))
				{
					OnPeriodChanged();
				}
			}
		}

		/// <summary>
		/// Конец периода (дата МЛ), включительно
		/// </summary>
		[PropertyChangedAlso(nameof(IsPeriodSelected), nameof(CanChangeFilters), nameof(CanGenerateReport))]
		public DateTime? EndDate
		{
			get => _endDate;
			set
			{
				if(SetField(ref _endDate, value))
				{
					OnPeriodChanged();
				}
			}
		}

		/// <summary>
		/// Сформированный отчет
		/// </summary>
		[PropertyChangedAlso(nameof(CanExportReport))]
		public CompletedAddressesReport Report
		{
			get => _report;
			private set => SetField(ref _report, value);
		}

		/// <summary>
		/// Идет формирование отчета
		/// </summary>
		[PropertyChangedAlso(
			nameof(CanChangeFilters),
			nameof(CanChangePeriod),
			nameof(CanGenerateReport),
			nameof(CanAbortReport),
			nameof(CanExportReport))]
		public bool IsReportGenerationInProgress
		{
			get => _isReportGenerationInProgress;
			private set => SetField(ref _isReportGenerationInProgress, value);
		}

		/// <summary>
		/// Идет загрузка списков водителей и районов выбранного периода
		/// </summary>
		[PropertyChangedAlso(nameof(CanChangeFilters), nameof(CanGenerateReport))]
		public bool IsFilterListsLoading
		{
			get => _isFilterListsLoading;
			private set => SetField(ref _isFilterListsLoading, value);
		}

		/// <summary>
		/// Период выбран: обе даты заданы, начало не позже конца
		/// </summary>
		public bool IsPeriodSelected => StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date <= EndDate.Value.Date;

		public bool CanChangeFilters => IsPeriodSelected && !IsFilterListsLoading && !IsReportGenerationInProgress;

		public bool CanChangePeriod => !IsReportGenerationInProgress;

		public bool CanGenerateReport => CanChangeFilters;

		public bool CanAbortReport => IsReportGenerationInProgress;

		public bool CanExportReport => !IsReportGenerationInProgress && Report != null && Report.Rows.Any();

		private void OnPeriodChanged()
		{
			_filterListsCancellationTokenSource?.Cancel();

			if(IsPeriodSelected)
			{
				_filterListsLoadingTask = ReloadFilterListsAsync();
				return;
			}

			IsFilterListsLoading = false;
			ApplyFilterLists(new List<CompletedAddressesCountNode>());
		}

		private async Task ReloadFilterListsAsync()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			_filterListsCancellationTokenSource = cancellationTokenSource;

			var startDate = StartDate.Value;
			var endDate = EndDate.Value;

			IsFilterListsLoading = true;

			try
			{
				IList<CompletedAddressesCountNode> nodes;

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Списки фильтров отчета по выполненным адресам"))
				{
					nodes = await _routeListItemRepository.GetCompletedAddressesCountsByDriverAndDistrictAsync(
						uow, startDate, endDate, cancellationTokenSource.Token);
				}

				cancellationTokenSource.Token.ThrowIfCancellationRequested();

				_guiDispatcher.RunInGuiTread(() =>
				{
					if(!cancellationTokenSource.IsCancellationRequested)
					{
						ApplyFilterLists(nodes);
					}
				});
			}
			catch(OperationCanceledException)
			{
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при загрузке списков фильтров отчета по выполненным адресам");

				_guiDispatcher.RunInGuiTread(() =>
				{
					if(!cancellationTokenSource.IsCancellationRequested)
					{
						ApplyFilterLists(new List<CompletedAddressesCountNode>());
					}

					_interactiveService.ShowMessage(
						ImportanceLevel.Error, $"При загрузке списков фильтров возникла ошибка:\n{ex.Message}");
				});
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					if(ReferenceEquals(_filterListsCancellationTokenSource, cancellationTokenSource))
					{
						IsFilterListsLoading = false;
					}
				});
			}
		}

		/// <summary>
		/// Подставляет списки периода в фильтр. Выполняется в потоке GUI
		/// </summary>
		private void ApplyFilterLists(IList<CompletedAddressesCountNode> nodes)
		{
			_drivers = CompletedAddressesReport.GetDrivers(nodes);
			_districts = CompletedAddressesReport.GetDistricts(nodes);

			var selection = GetSelectionToApply(nodes.Count > 0);

			ApplySelection(
				_driversFilter,
				_drivers,
				selection.IncludedDriverIds.Select(x => x.ToString()),
				selection.ExcludedDriverIds.Select(x => x.ToString()));

			ApplySelection(
				_districtsFilter,
				_districts,
				selection.IncludedDistrictKeys,
				selection.ExcludedDistrictKeys);

			_driversFilter.RefreshFilteredElements();
			_districtsFilter.RefreshFilteredElements();
			FilterViewModel.RefreshActiveFilterElements();
		}

		private CompletedAddressesReportFilterSelection GetSelectionToApply(bool hasData)
		{
			if(_isSavedSelectionApplied || !hasData)
			{
				return GetCurrentSelection();
			}

			_isSavedSelectionApplied = true;

			if(!CompletedAddressesReportFilterSelection.TryParse(
				_userSettingsService.Settings.CompletedAddressesReportFilterSelectionJson, out var saved))
			{
				_logger.LogWarning("Не удалось разобрать сохраненный выбор фильтров отчета по выполненным адресам");
			}

			return saved;
		}

		private static void ApplySelection(
			IncludeExcludeBoolParamsFilter filter,
			IList<(string Key, string Title)> items,
			IEnumerable<string> includedKeys,
			IEnumerable<string> excludedKeys)
		{
			var available = new Dictionary<string, (string Key, string Title)>(StringComparer.OrdinalIgnoreCase);

			foreach(var item in items)
			{
				available[item.Key] = item;
			}

			filter.IncludedElements.Clear();
			filter.ExcludedElements.Clear();

			foreach(var key in includedKeys.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				if(available.TryGetValue(key, out var item))
				{
					filter.IncludedElements.Add(new IncludeExcludeElement<string, string> { Id = item.Key, Title = item.Title });
				}
			}

			foreach(var key in excludedKeys.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				if(available.TryGetValue(key, out var item))
				{
					filter.ExcludedElements.Add(new IncludeExcludeElement<string, string> { Id = item.Key, Title = item.Title });
				}
			}
		}

		private CompletedAddressesReportFilterSelection GetCurrentSelection()
		{
			var selection = new CompletedAddressesReportFilterSelection();

			selection.IncludedDriverIds.AddRange(_driversFilter.IncludedElements.Select(x => int.Parse(x.Number)));
			selection.ExcludedDriverIds.AddRange(_driversFilter.ExcludedElements.Select(x => int.Parse(x.Number)));
			selection.IncludedDistrictKeys.AddRange(_districtsFilter.IncludedElements.Select(x => x.Number));
			selection.ExcludedDistrictKeys.AddRange(_districtsFilter.ExcludedElements.Select(x => x.Number));

			return selection;
		}

		private async Task GenerateReport(CancellationToken token)
		{
			if(!CanGenerateReport)
			{
				return;
			}

			var startDate = StartDate.Value;
			var endDate = EndDate.Value;
			var selection = GetCurrentSelection();

			SaveFilterSelection(selection);

			IsReportGenerationInProgress = true;

			var cancellationTokenSource = new CancellationTokenSource();
			_reportCancellationTokenSource = cancellationTokenSource;

			try
			{
				IList<CompletedAddressesCountNode> nodes;

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Отчет по выполненным адресам и районам"))
				{
					nodes = await _routeListItemRepository.GetCompletedAddressesCountsByDriverAndDistrictAsync(
						uow, startDate, endDate, cancellationTokenSource.Token);
				}

				cancellationTokenSource.Token.ThrowIfCancellationRequested();

				var report = CompletedAddressesReport.Create(startDate, endDate, nodes, selection);

				_guiDispatcher.RunInGuiTread(() => Report = report);
			}
			catch(OperationCanceledException ex)
			{
				LogErrorAndShowMessageInGuiThread(ex, "Формирование отчета было прервано вручную");
			}
			catch(Exception ex)
			{
				LogErrorAndShowMessageInGuiThread(ex, $"При формировании отчета возникла ошибка:\n{ex.Message}");
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() => IsReportGenerationInProgress = false);

				if(ReferenceEquals(_reportCancellationTokenSource, cancellationTokenSource))
				{
					_reportCancellationTokenSource = null;
				}

				cancellationTokenSource.Dispose();
			}
		}

		private void AbortReportGeneration()
		{
			var cancellationTokenSource = _reportCancellationTokenSource;

			if(!IsReportGenerationInProgress || cancellationTokenSource == null)
			{
				return;
			}

			try
			{
				cancellationTokenSource.Cancel();
			}
			catch(ObjectDisposedException)
			{
			}
		}

		/// <summary>
		/// Сохраняет выбор фильтров пользователя целиком (прежнее значение не сливается с новым)
		/// </summary>
		private void SaveFilterSelection(CompletedAddressesReportFilterSelection selection)
		{
			var json = selection.ToJson();

			try
			{
				_userSettingsService.Settings.CompletedAddressesReportFilterSelectionJson = json;

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сохранение выбора фильтров отчета по выполненным адресам"))
				{
					var settings = _userRepository.GetUserSettings(uow, _userService.CurrentUserId);

					if(settings != null)
					{
						settings.CompletedAddressesReportFilterSelectionJson = json;
						uow.Save(settings);
						uow.Commit();
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении выбора фильтров отчета по выполненным адресам");

				_interactiveService.ShowMessage(
					ImportanceLevel.Warning, $"Не удалось сохранить выбор фильтров:\n{ex.Message}");
			}
		}

		private void ExportReport()
		{
			if(!CanExportReport)
			{
				return;
			}

			var dialogSettings = CreateDialogSettings();

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!saveDialogResult.Successful)
			{
				return;
			}

			try
			{
				Report.ExportToExcel(saveDialogResult.Path);

				_interactiveService.ShowMessage(ImportanceLevel.Info, "Сохранение отчёта завершено.");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выгрузке отчета по выполненным адресам в Excel");

				_interactiveService.ShowMessage(ImportanceLevel.Error, $"При сохранении отчета возникла ошибка:\n{ex.Message}");
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
				FileName = $"Отчет по выполненным адресам и районам {Report.StartDate:dd.MM.yyyy}-{Report.EndDate:dd.MM.yyyy}{reportFileExtension}"
			};

			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Отчет Excel", "*" + reportFileExtension));

			return dialogSettings;
		}

		private void LogErrorAndShowMessageInGuiThread(Exception ex, string message)
		{
			_logger.LogError(ex, "Ошибка формирования отчета по выполненным адресам: {Message}", message);

			_guiDispatcher.RunInGuiTread(() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);
			});
		}

		public override void Dispose()
		{
			_reportCancellationTokenSource?.Cancel();
			_filterListsCancellationTokenSource?.Cancel();

			base.Dispose();
		}
	}
}
