using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Tdi;
using QS.Utilities.Enums;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.ViewModels.Fuel.FuelCards;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Transport.Reports.IncorrectFuel
{
	public class IncorrectFuelReportViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		private readonly ILogger<IncorrectFuelReportViewModel> _logger;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly ViewModelEEVMBuilder<Car> _carEEVMBuilder;
		private readonly ViewModelEEVMBuilder<FuelCard> _fuelCardEEVMBuilder;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Car _car;
		private FuelCard _fuelCard;
		private bool _isExcludeOfficeWorkers;
		private IList<CarOwnType> _carOwnTypes;
		private IList<CarTypeOfUse> _carTypesOfUse;
		private IncorrectFuelReport _report;
		private bool _isReportGenerationInProgress;
		private CancellationTokenSource _cancellationTokenSource;

		public IncorrectFuelReportViewModel(
			ILogger<IncorrectFuelReportViewModel> logger,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ViewModelEEVMBuilder<Car> carEEVMBuilder,
			ViewModelEEVMBuilder<FuelCard> fuelCardEEVMBuilder,
			IGuiDispatcher guiDispatcher,
			IFileDialogService fileDialogService,
			IDialogSettingsFactory dialogSettingsFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_carEEVMBuilder = carEEVMBuilder ?? throw new ArgumentNullException(nameof(carEEVMBuilder));
			_fuelCardEEVMBuilder = fuelCardEEVMBuilder ?? throw new ArgumentNullException(nameof(fuelCardEEVMBuilder));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			Title = "Отчет по заправкам некорректным типом топлива";

			CarEntityEntryViewModel = CreateCarEntityEntryViewModel();
			FuelCardEntityEntryViewModel = CreateFuelCardEntityEntryViewModel();

			SetDefaultValuesInFilter();

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

		public IEntityEntryViewModel CarEntityEntryViewModel { get; }
		public IEntityEntryViewModel FuelCardEntityEntryViewModel { get; }

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

		public Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public FuelCard FuelCard
		{
			get => _fuelCard;
			set => SetField(ref _fuelCard, value);
		}

		public bool IsExcludeOfficeWorkers
		{
			get => _isExcludeOfficeWorkers;
			set => SetField(ref _isExcludeOfficeWorkers, value);
		}

		public IList<CarTypeOfUse> CarTypesOfUse
		{
			get => _carTypesOfUse;
			set => SetField(ref _carTypesOfUse, value);
		}

		public IList<CarOwnType> CarOwnTypes
		{
			get => _carOwnTypes;
			set => SetField(ref _carOwnTypes, value);
		}

		public IncorrectFuelReport Report
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

		private void SetDefaultValuesInFilter()
		{
			var yesterdayDate = DateTime.Today.AddDays(-1);

			StartDate = yesterdayDate;
			EndDate = yesterdayDate;
			IsExcludeOfficeWorkers = true;

			_carOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			_carTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>().ToList(); ;
		}

		private IEntityEntryViewModel CreateCarEntityEntryViewModel()
		{
			var viewModel = _carEEVMBuilder
					.SetViewModel(this)
					.SetUnitOfWork(UoW)
					.ForProperty(this, x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<CarViewModel>()
					.Finish();

			viewModel.CanViewEntity =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		private IEntityEntryViewModel CreateFuelCardEntityEntryViewModel()
		{
			var viewModel = _fuelCardEEVMBuilder
					.SetViewModel(this)
					.SetUnitOfWork(UoW)
					.ForProperty(this, x => x.FuelCard)
					.UseViewModelJournalAndAutocompleter<FuelCardJournalViewModel, FuelCardJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<FuelCardViewModel>()
					.Finish();

			viewModel.CanViewEntity =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FuelCard)).CanUpdate;

			return viewModel;
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

			_logger.LogInformation(
				"Формируем отчет по заправкам некорретным типом топлива.\nStartDate: {StartDate}, EndDate: {EndDate}",
				StartDate.Value,
				EndDate.Value);

			IsReportGenerationInProgress = true;
			Report = null;
			IncorrectFuelReport report = null;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				report = await IncorrectFuelReport.Create(
					UoW,
					StartDate.Value,
					EndDate.Value,
					Car?.Id,
					FuelCard?.Id,
					CarOwnTypes,
					CarTypesOfUse,
					IsExcludeOfficeWorkers,
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
	}
}
