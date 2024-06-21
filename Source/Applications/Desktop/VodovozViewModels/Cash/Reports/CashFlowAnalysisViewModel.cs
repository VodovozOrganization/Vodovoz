using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel : DialogTabViewModelBase
	{
		private readonly Color _accentColor = Color.FromArgb(249, 191, 143);

		private readonly IFileDialogService _fileDialogService;
		private readonly ICommonServices _commonServices;
		private readonly CashFlowDdsReportRenderer _cashFlowDdsReportRenderer;
		private bool _canGenerateDdsReport = true;

		private DateTime _startDate;
		private DateTime _endDate;
		private CashFlowDdsReport _report;
		private bool _hideCategoriesWithoutDocuments;
		private CashFlowDdsReport.ReportMode _reportMode;

		public CashFlowAnalysisViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			CashFlowDdsReportRenderer cashFlowDdsReportRenderer,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_cashFlowDdsReportRenderer = cashFlowDdsReportRenderer ?? throw new ArgumentNullException(nameof(cashFlowDdsReportRenderer));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			var now = DateTime.Now;

			StartDate = now.Date;
			EndDate = now.LatestDayTime();

			TabName = "Анализ движения денежных средств";

			GenerateDdsReportCommand = new DelegateCommand(GenerateCashFlowDdsReport, () => CanGenerateDdsReport);

			SaveReportCommand = new DelegateCommand(SaveReport, () => CanSaveReport);

			ShowDdsReportInfoCommand = new DelegateCommand(ShowCashFlowDdsReportInfo);
		}

		public DelegateCommand GenerateDdsReportCommand { get; }

		public DelegateCommand SaveReportCommand { get; }

		public DelegateCommand ShowDdsReportInfoCommand { get; }

		[PropertyChangedAlso(nameof(CanSaveReport))]
		public CashFlowDdsReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public bool CanGenerateDdsReport
		{
			get => _canGenerateDdsReport;
			private set => SetField(ref _canGenerateDdsReport, value);
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

		public bool HideCategoriesWithoutDocuments
		{
			get => _hideCategoriesWithoutDocuments;
			set => SetField(ref _hideCategoriesWithoutDocuments, value);
		}

		public CashFlowDdsReport.ReportMode ReportMode
		{
			get => _reportMode;
			set
			{
				if(SetField(ref _reportMode, value))
				{
					if(value == CashFlowDdsReport.ReportMode.Dds)
					{
						TabName = "Анализ движения денежных средств";
					}
					else
					{
						TabName = "Анализ движения денежных расходов";
					}
				}
			}
		}

		public bool CanSaveReport => Report != null;

		public Color AccentColor => _accentColor;

		public void SaveReport()
		{
			var path = RunSaveAsDialog();

			if(string.IsNullOrWhiteSpace(path))
			{
				CanGenerateDdsReport = true;
				return;
			}

			RenderCashFlowDdsReport(Report, path);

			RunOpenDialog(path);
		}

		private string RunSaveAsDialog()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = GetReportFilename(),
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(result.Successful)
			{
				return result.Path;
			}

			return null;
		}

		private void RenderCashFlowDdsReport(CashFlowDdsReport report, string path)
		{
			var rendered = _cashFlowDdsReportRenderer.Render(report, _accentColor);
			rendered.SaveAs(path);
		}

		private void GenerateCashFlowDdsReport()
		{
			CanGenerateDdsReport = false;

			Report = CashFlowDdsReport.GenerateReport(UoW, StartDate, EndDate, HideCategoriesWithoutDocuments, ReportMode);

			if(Report != null)
			{
				TabName = $"Анализ движения денежных средств c {Report.StartDate:dd.MM.yyyy} по {Report.EndDate:dd.MM.yyyy}";
			}

			CanGenerateDdsReport = true;
		}

		private void RunOpenDialog(string path)
		{
			if(_commonServices.InteractiveService.Question(
				"Открыть отчет?",
				"Отчет сохранен"))
			{
				Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
			}
		}

		private void ShowCashFlowDdsReportInfo()
		{
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
				"Отчет ДДС генерируется только при наличии права \"Доступен отчет ДДС\"\n" +
				"В отчет не входят статьи, в которых установлена галочка \"Не включать в ДДС\"\n" +
				"В отчет не входят архивные статьи\n" +
				"Отчет генерируется в зависимости от указанного интервала дат, другие параметры на отчет не влияют\n" +
				"Генерация отчета может бликировать работу программы ДВ на время генерации\n" +
				"Отчет разбит по группам статей, затем на типы операций по статьям\n" +
				"Отчет может генерироваться только при уровне вложенности статей 7 (6 вложенных друг в друга групп и статья, из-за ограничения Excel)",
				"Информация по отчету ДДС");
		}

		private string GetReportFilename() => $"{TabName} от {Report.CreatedAt:yyyy-MM-dd-HH-mm}.xlsx";
	}
}
