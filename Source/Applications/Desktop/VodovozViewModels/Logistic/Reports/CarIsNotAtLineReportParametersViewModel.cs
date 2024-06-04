using ClosedXML.Report;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Logistic;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReportParametersViewModel : DialogViewModelBase
	{
		private DateTime _date;
		private int _countDays;

		private CarIsNotAtLineReport _report;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IGenericRepository<CarEvent> _carEventRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _uUnitOfWork;

		public CarIsNotAtLineReportParametersViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService,
			IGenericRepository<CarEventType> carEventTypeRepository,
			IGenericRepository<CarEvent> carEventRepository,
			IncludeExludeFilterGroupViewModel includeExludeFilterGroupViewModel,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_dialogSettingsFactory = dialogSettingsFactory
				?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_carEventRepository = carEventRepository
				?? throw new ArgumentNullException(nameof(carEventRepository));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));

			Title = typeof(CarIsNotAtLineReport).GetClassUserFriendlyName().Nominative;

			_uUnitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title);

			Date = DateTime.Today;
			CountDays = 4;

			includeExludeFilterGroupViewModel.InitializeFor(_uUnitOfWork, carEventTypeRepository);
			includeExludeFilterGroupViewModel.RefreshFilteredElementsCommand.Execute();

			IncludeExludeFilterGroupViewModel = includeExludeFilterGroupViewModel;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			ExportReportCommand = new DelegateCommand(ExportReport);
			GenerateAndSaveReportCommand = new DelegateCommand(GenerateAndSaveReport);
		}

		public IncludeExludeFilterGroupViewModel IncludeExludeFilterGroupViewModel { get; }

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand ExportReportCommand { get; }
		public DelegateCommand GenerateAndSaveReportCommand { get; }

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

		private void GenerateReport()
		{
			var reportName = typeof(CarIsNotAtLineReport).GetClassUserFriendlyName().Nominative;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(reportName))
			{
				CarIsNotAtLineReport report = null;

				var reportResult = CarIsNotAtLineReport.Generate(
					unitOfWork,
					_carEventRepository,
					Date,
					CountDays,
					IncludeExludeFilterGroupViewModel.IncludedElements.Select(e => (int.Parse(e.Number), e.Title)),
					IncludeExludeFilterGroupViewModel.ExcludedElements.Select(e => (int.Parse(e.Number), e.Title)));

				reportResult.Match(
					r => report = r,
					errors => _interactiveService.ShowMessage(
						ImportanceLevel.Error,
						string.Join("\n", errors.Select(e => e.Message)),
						"Ошибка при формировании отчета"));

				_report = report;
			}
		}

		private void ExportReport()
		{
			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(_report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				PostProcess(_report)
					.Export(saveDialogResult.Path);
			}
		}

		private XLTemplate PostProcess(CarIsNotAtLineReport report)
		{
			var template = report.GetRawTemplate();

			if(!report.CarReceptionRows.Any())
			{
				template.Workbook.Worksheet(1).Rows(12, 14).Delete();
			}

			if(!report.CarTransferRows.Any())
			{
				template.Workbook.Worksheet(1).Rows(7, 10).Delete();
			}

			return template.RenderTemplate(report);
		}

		private void GenerateAndSaveReport()
		{
			GenerateReportCommand.Execute();

			if(_report != null)
			{
				ExportReportCommand.Execute();
			}
		}
	}
}
