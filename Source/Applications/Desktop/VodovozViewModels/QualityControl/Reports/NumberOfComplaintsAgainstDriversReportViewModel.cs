using ClosedXML.Excel;
using ClosedXML.Report;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\QualityControl\NumberOfComplaintsAgainstDriversReport.xlsx";

		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private NumberOfComplaintsAgainstDriversReport _report;
		private DateTime? _startDate;
		private DateTime? _endDate;

		public NumberOfComplaintsAgainstDriversReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService;
			TabName = typeof(NumberOfComplaintsAgainstDriversReport).GetClassUserFriendlyName().Nominative;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			ExportReportCommand = new DelegateCommand(ExportReport, () => CanExportReport);
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

		[PropertyChangedAlso(nameof(CanExportReport))]
		public NumberOfComplaintsAgainstDriversReport Report
		{
			get => _report;
			private set => SetField(ref _report, value);
		}

		public bool CanExportReport => Report != null;

		public DelegateCommand GenerateReportCommand { get; }

		public DelegateCommand ExportReportCommand { get; }

		private void GenerateReport()
		{
			if(StartDate is null || EndDate is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не указан период");
				return;
			}

			Report = NumberOfComplaintsAgainstDriversReport.Generate(UoW, StartDate.Value, EndDate.Value);
		}

		private void ExportReport()
		{
			var reportName = typeof(NumberOfComplaintsAgainstDriversReport).GetClassUserFriendlyName().Nominative;

			var dialogSettings = new DialogSettings
			{
				Title = $"Сохранить {reportName}",
				DefaultFileExtention = ".xlsx",
				FileName = $"{reportName} {DateTime.Now:dd-MM-yyyy-HH-mm}",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			var template = new XLTemplate(_templatePath);

			template.AddVariable(Report);
			template.Generate();

			foreach(var worksheet in template.Workbook.Worksheets)
			{
				foreach(var row in worksheet.Rows())
				{
					row.AdjustToContents(3, 3);
					row.ClearHeight();
				}
			}

			template.SaveAs(result.Path);
		}
	}
}
