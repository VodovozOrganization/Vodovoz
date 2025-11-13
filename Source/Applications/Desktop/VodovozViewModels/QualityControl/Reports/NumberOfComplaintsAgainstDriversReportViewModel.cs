using ClosedXML.Report;
using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Settings.Complaints;
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
		private GeoGroup _selectedGeoGroup;
		private ComplaintResultBase _selectedComplaintResult;
		private ReportSortOrder _selectedReportSortOrder;
		private IncludeExludeFiltersViewModel _includeExcludeFilterViewModel;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;

		public NumberOfComplaintsAgainstDriversReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IComplaintSettings complaintSettings,
			IGenericRepository<Subdivision> subdivisionRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_fileDialogService = fileDialogService;
			TabName = typeof(NumberOfComplaintsAgainstDriversReport).GetClassUserFriendlyName().Nominative;

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			ExportReportCommand = new DelegateCommand(ExportReport, () => CanExportReport);
			GeoGroups = UoW.GetAll<GeoGroup>().ToList();
			ComplaintResults = UoW.GetAll<ComplaintResultBase>().ToList();
			SelectedComplaintResult = ComplaintResults.FirstOrDefault(x => x.Id == complaintSettings.GuiltProvenComplaintResultId);
			SetupFilter();
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

		public GeoGroup SelectedGeoGroup
		{
			get => _selectedGeoGroup;
			set => SetField(ref _selectedGeoGroup, value);
		}

		public ComplaintResultBase SelectedComplaintResult
		{
			get => _selectedComplaintResult;
			set => SetField(ref _selectedComplaintResult, value);
		}

		public ReportSortOrder SelectedReportSortOrder
		{
			get => _selectedReportSortOrder;
			set => SetField(ref _selectedReportSortOrder, value);
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
		public IList<GeoGroup> GeoGroups { get; }
		public IList<ComplaintResultBase> ComplaintResults { get; }

		public IncludeExludeFiltersViewModel IncludeExcludeFilterViewModel => _includeExcludeFilterViewModel;

		private void SetupFilter()
		{
			_includeExcludeFilterViewModel = new IncludeExludeFiltersViewModel(_interactiveService);
			_includeExcludeFilterViewModel.AddFilter(UoW, _subdivisionRepository);
		}

		private void GenerateReport()
		{
			if(StartDate is null || EndDate is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не указан период");
				return;
			}

			var selectedGeoGroupId = SelectedGeoGroup?.Id ?? 0;
			var selectedComplaintResultId = SelectedComplaintResult?.Id ?? 0;

			Report = NumberOfComplaintsAgainstDriversReport.Generate(UoW, StartDate.Value, EndDate.Value.LatestDayTime(), selectedGeoGroupId,
				selectedComplaintResultId, SelectedReportSortOrder, _includeExcludeFilterViewModel);
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
