using DateTimeHelpers;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public class TurnoverOfWarehouseBalancesReportViewModel : DialogViewModelBase
	{
		private readonly ILogger<TurnoverOfWarehouseBalancesReportViewModel> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IGuiDispatcher _guiDispatcher;
		private IncludeExludeFiltersViewModel _filterViewModel;
		private IUnitOfWork _unitOfWork;
		
		private TurnoverOfWarehouseBalancesReport _report;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private DateTimeSliceType _slice;
		private bool _isReportGenerating;
		private bool _canCancelGenerateReport;
		private IEnumerable<Error> _lastGenerationErrors;
		private bool _canGenerateReport;
		private bool _canSave;
		private bool _isGenerating;
		private DateTimeSliceType _slicingType;

		public TurnoverOfWarehouseBalancesReportViewModel(
			ILogger<TurnoverOfWarehouseBalancesReportViewModel> logger,
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService,
			IGuiDispatcher guiDispatcher)
			: base(navigation)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory
				?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			_dialogSettingsFactory = dialogSettingsFactory
				?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_guiDispatcher = guiDispatcher
				?? throw new ArgumentNullException(nameof(guiDispatcher));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			Title = "Оборачиваемость складских остатков";

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			ConfigureFilter();

			GenerateReportCommand = new AsyncCommand(guiDispatcher, GenerateReportAsync, () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, x => x.CanGenerateReport);

			AbortCreateCommand = new DelegateCommand(AbortCreate, () => CanCancelGenerateReport);
			AbortCreateCommand.CanExecuteChangedWith(this, x => x.CanCancelGenerateReport);

			SaveCommand = new DelegateCommand(Save, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			ShowInfoCommand = new DelegateCommand(ShowInfo);

			CanGenerateReport = true;
		}

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		public TurnoverOfWarehouseBalancesReport Report
		{
			get => _report;
			set
			{
				if(SetField(ref _report, value))
				{
					CanSave = value != null;
				}
			}
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

		public DateTimeSliceType SlicingType
		{
			get => _slicingType;
			set => SetField(ref _slicingType, value);
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

		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set => SetField(ref _isGenerating, value);
		}

		public AsyncCommand GenerateReportCommand { get; }
		public DelegateCommand ShowInfoCommand { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand AbortCreateCommand { get; }

		private void Save()
		{
			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(_report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				_report.RenderTemplate().Export(saveDialogResult.Path);
			}
		}

		private void AbortCreate()
		{
			CanCancelGenerateReport = false;
			GenerateReportCommand.Abort();
		}

		private void ShowInfo()
		{
			var info =
				"1. Настройки отчёта:\n" +
				"«В разрезе» - Выбор разбивки по периодам. В отчет попадают периоды согласно выбранного разреза, но не выходя за границы выставленного периода.\n";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private void ConfigureFilter()
		{
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateTurnoverOfWarehouseBalancesReportFilterViewModel(_unitOfWork);
		}

		private async Task GenerateReportAsync(CancellationToken cancellationToken)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CanGenerateReport = false;
				CanCancelGenerateReport = true;
			});

			#region Сбор параметров

			var warehouseFilter = _filterViewModel.GetFilter<IncludeExcludeEntityFilter<Warehouse>>();

			var selectedIncludedWarehouseIds = warehouseFilter.GetIncluded().ToArray();
			var selectedExcludedWarehouseIds = warehouseFilter.GetExcluded().ToArray();

			var nomenclaturesCategoriesFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<NomenclatureCategory>>();
			var includedNomenclatureCategoryIds = nomenclaturesCategoriesFilter.GetIncluded().ToArray();
			var excludedNomenclatureCategoryIds = nomenclaturesCategoriesFilter.GetExcluded().ToArray();

			var nomenclaturesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();
			var includedNomenclatureIds = nomenclaturesFilter.GetIncluded().ToArray();
			var excludedNomenclatureIds = nomenclaturesFilter.GetExcluded().ToArray();

			var productGroupsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();
			var includedProductGroupIds = productGroupsFilter.GetIncluded().ToArray();
			var excludedProductGroupIds = productGroupsFilter.GetExcluded().ToArray();

			#endregion Сбор параметров

			var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title + " - генерация отчета");

			try
			{
				var reportResult = await TurnoverOfWarehouseBalancesReport.Generate(
					unitOfWork,
					StartDate.Value,
					EndDate.Value,
					SlicingType,
					selectedIncludedWarehouseIds,
					selectedExcludedWarehouseIds,
					includedNomenclatureCategoryIds,
					excludedNomenclatureCategoryIds,
					includedNomenclatureIds,
					excludedNomenclatureIds,
					includedProductGroupIds,
					excludedProductGroupIds,
					cancellationToken);

				_guiDispatcher.RunInGuiTread(() =>
				{
					reportResult.Match(
						x => Report = x,
						errors => ShowErrors(errors));
				});
			}
			catch(OperationCanceledException)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					ShowWarning("Формирование отчета было прервано");
				});
			}
			catch(Exception e)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_logger.LogError(e, e.Message);
					ShowError(e.Message);
				});
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					CanGenerateReport = true;
					CanCancelGenerateReport = false;
				});

				unitOfWork?.Session?.Clear();
				unitOfWork?.Dispose();
			}
		}

		private void ShowErrors(IEnumerable<Error> errors)
		{
			ShowError(string.Join(
				"\n",
				errors.Select(e => e.Message)));
		}

		private void ShowError(string error)
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				error,
				"Ошибка при формировании отчета!");
		}

		private void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}
	}
}
