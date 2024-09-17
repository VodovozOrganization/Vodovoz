using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.EntityRepositories.Complaints;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public class OksDailyReportViewModel : UowDialogViewModelBase
	{
		private readonly ILogger<OksDailyReportViewModel> _logger;
		private readonly IFileDialogService _fileDialogService;
		private readonly IComplaintsRepository _complaintsRepository;
		private DateTime _date = DateTime.Today.AddDays(-1);

		public OksDailyReportViewModel(
			ILogger<OksDailyReportViewModel> logger,
			IFileDialogService fileDialogService,
			IComplaintsRepository complaintsRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation
			) : base(unitOfWorkFactory, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));

			CreateReportCommand = new DelegateCommand(CreateReport);
		}

		public DelegateCommand CreateReportCommand { get; }

		public DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		private void CreateReport()
		{
			var complaints = _complaintsRepository.GetClientComplaintsForPeriod(UoW, Date, Date);

			var dialogSettings = GetSaveExcelReportDialogSettings();
			var saveFileDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!saveFileDialogResult.Successful)
			{
				return;
			}

			var report = OksDailyReport.Create();
		}

		private DialogSettings GetSaveExcelReportDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"{OksDailyReport.GetReportTitle(Date)}.xlsx"
			};

			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			return dialogSettings;
		}
	}
}
