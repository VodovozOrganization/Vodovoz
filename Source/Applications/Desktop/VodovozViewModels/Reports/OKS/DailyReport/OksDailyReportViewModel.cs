using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Settings.Complaints;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public class OksDailyReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<OksDailyReportViewModel> _logger;
		private readonly IFileDialogService _fileDialogService;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IComplaintSettings _complaintSettings;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private DateTime _date = new DateTime(2023, 05, 22); //DateTime.Today.AddDays(-1);

		public OksDailyReportViewModel(
			ILogger<OksDailyReportViewModel> logger,
			IFileDialogService fileDialogService,
			IComplaintsRepository complaintsRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IOrderRepository orderRepository,
			IComplaintSettings complaintSettings,
			ISubdivisionSettings subdivisionSettings,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_complaintSettings = complaintSettings ?? throw new ArgumentNullException(nameof(complaintSettings));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			Title = "Ежедневный отчет ОКС";

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
			var dialogSettings = GetSaveExcelReportDialogSettings();
			var saveFileDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!saveFileDialogResult.Successful)
			{
				return;
			}

			var report = OksDailyReport.Create(
				UoW,
				Date,
				_complaintsRepository,
				_undeliveredOrdersRepository,
				_orderRepository,
				_complaintSettings,
				_subdivisionSettings);

			report.ExportReport(saveFileDialogResult.Path);
		}

		private DialogSettings GetSaveExcelReportDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"Ежедневный отчет ОКС за {Date.ToString("dd.MM.yyyy")}.xlsx"
			};

			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			return dialogSettings;
		}
	}
}
