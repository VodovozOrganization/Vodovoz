using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Store.Reports
{
	public class DefectiveItemsReportViewModel : DialogViewModelBase, IDisposable
	{
		private readonly ILogger<DefectiveItemsReportViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IGuiDispatcher _guiDispatcher;
		private DefectiveItemsReport _report;

		private bool _canGenerateReport;
		private bool _canCancelGenerateReport;
		private bool _isReportGenerating;
		private bool _canSave;
		private bool _isGenerating;

		private Employee _driver;
		private DateTime _startDate;
		private DateTime _endDate;
		private DefectSource? _defectSource;
		private bool _isWarehouseEnabled;
		private Warehouse _selectedWarehouse;

		public DefectiveItemsReportViewModel(
			ILogger<DefectiveItemsReportViewModel> logger,
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ViewModelEEVMBuilder<Employee> employeeViewModelEEVMBuilder,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService,
			IGuiDispatcher guiDispatcher)
			: base(navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(employeeViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(employeeViewModelEEVMBuilder));
			}

			Title = typeof(DefectiveItemsReport).GetClassUserFriendlyName().Nominative.CapitalizeSentence(); ;

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot("Отчет по браку");

			DriverViewModel = employeeViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UnitOfWork)
				.ForProperty(this, x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(x => x.Category = Core.Domain.Employees.EmployeeCategory.driver)
				.UseViewModelDialog<EmployeeViewModel>()
			.Finish();

			GenerateReportCommand = new AsyncCommand(guiDispatcher, GenerateReportAsync, () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, x => x.CanGenerateReport);

			AbortCreateCommand = new DelegateCommand(AbortCreate, () => CanCancelGenerateReport);
			AbortCreateCommand.CanExecuteChangedWith(this, x => x.CanCancelGenerateReport);

			SaveCommand = new DelegateCommand(Save, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);
		
			CanGenerateReport = true;
		}

		public DefectiveItemsReport Report
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

		public Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public DefectSource? DefectSource
		{
			get => _defectSource;
			set => SetField(ref _defectSource, value);
		}

		public IEntityEntryViewModel DriverViewModel { get; }
		public AsyncCommand GenerateReportCommand { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand AbortCreateCommand { get; }
		public IUnitOfWork UnitOfWork { get; private set; }

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

		public bool IsWarehouseEnabled
		{
			get => _isWarehouseEnabled;
			set => SetField(ref _isWarehouseEnabled, value);
		}

		public Warehouse SelectedWarehouse
		{
			get => _selectedWarehouse;
			set => SetField(ref _selectedWarehouse, value);
		}

		private async Task GenerateReportAsync(CancellationToken cancellationToken)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CanGenerateReport = false;
				CanCancelGenerateReport = true;
			});

			try
			{
				var reportResult = await DefectiveItemsReport.Create(UnitOfWork, StartDate, EndDate, DefectSource, Driver?.Id, cancellationToken, IsWarehouseEnabled ? SelectedWarehouse : null);

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

				UnitOfWork.Session.Clear();
			}
		}

		private void AbortCreate()
		{
			CanCancelGenerateReport = false;
			GenerateReportCommand.Abort();
		}

		private void Save()
		{
			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(_report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				_report.RenderTemplate().Export(saveDialogResult.Path);
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

		public void Dispose()
		{
			UnitOfWork?.Dispose();
		}
	}
}
