using ClosedXML.Report;
using DateTimeHelpers;
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
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Errors;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public class OnlinePaymentsReportViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private DateTime _startDate;
		private DateTime _endDate;
		private bool _isDateTimeRangeYesterday;
		private bool _isDateTimeRangeLast3Days;
		private bool _isDateTimeRangeCustomPeriod;
		private OnlinePaymentsReport _report;
		private bool _canGenerateReport;
		private bool _isReportGenerating;
		private string _selectedShop;
		private bool _canSaveReport;
		private bool _canCancelGeneration;

		public OnlinePaymentsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IPaymentsRepository paymentsRepository,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService)
			: base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_paymentsRepository = paymentsRepository
				?? throw new ArgumentNullException(nameof(paymentsRepository));
			_guiDispatcher = guiDispatcher
				?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_dialogSettingsFactory = dialogSettingsFactory
				?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));

			Title = typeof(OnlinePaymentsReport).GetClassUserFriendlyName().Nominative;

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			Shops = _paymentsRepository.GetAllShopsFromTinkoff(_unitOfWork);

			IsDateTimeRangeYesterday = true;

			GenerateReportCommand = new AsyncCommand(GenerateReport, () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, vm => vm.CanGenerateReport);
			CanGenerateReport = true;

			ExportReportCommand = new DelegateCommand(ExportReport, () => CanSaveReport);
			ExportReportCommand.CanExecuteChangedWith(this, vm => vm.CanSaveReport);

			CancelGenerationCommand = new DelegateCommand(CancelGeneration, () => CanCancelGeneration);
			CancelGenerationCommand.CanExecuteChangedWith(this, vm => vm.CanCancelGeneration);
		}

		public IEnumerable<string> Shops { get; }

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

		public string SelectedShop
		{
			get => _selectedShop;
			set => SetField(ref _selectedShop, value);
		}

		public bool IsDateTimeRangeYesterday
		{
			get => _isDateTimeRangeYesterday;
			set
			{
				if(SetField(ref _isDateTimeRangeYesterday, value) && value)
				{
					IsDateTimeRangeLast3Days = false;
					IsDateTimeRangeCustomPeriod = false;
					StartDate = DateTime.Today.AddDays(-1);
					EndDate = DateTime.Today;
				}
			}
		}

		public bool IsDateTimeRangeLast3Days
		{
			get => _isDateTimeRangeLast3Days;
			set
			{
				if(SetField(ref _isDateTimeRangeLast3Days, value) && value)
				{
					IsDateTimeRangeYesterday = false;
					IsDateTimeRangeCustomPeriod = false;
					StartDate = DateTime.Today.AddDays(-3);
					EndDate = DateTime.Today;
				}
			}
		}

		public bool IsDateTimeRangeCustomPeriod
		{
			get => _isDateTimeRangeCustomPeriod;
			set
			{
				if(SetField(ref _isDateTimeRangeCustomPeriod, value) && value)
				{
					IsDateTimeRangeYesterday = false;
					IsDateTimeRangeLast3Days = false;
				}
			}
		}

		public OnlinePaymentsReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public IEnumerable<OnlinePaymentsReport.OrderRow> PaidOrders => Report?.PaidOrders ?? Enumerable.Empty<OnlinePaymentsReport.OrderRow>();

		public IEnumerable<OnlinePaymentsReport.OrderRow> PaymentMissingOrders => Report?.PaymentMissingOrders ?? Enumerable.Empty<OnlinePaymentsReport.OrderRow>();

		public IEnumerable<OnlinePaymentsReport.OrderRow> OverpaidOrders => Report?.OverpaidOrders ?? Enumerable.Empty<OnlinePaymentsReport.OrderRow>();

		public IEnumerable<OnlinePaymentsReport.OrderRow> UnderpaidOrders => Report?.UnderpaidOrders ?? Enumerable.Empty<OnlinePaymentsReport.OrderRow>();

		public IEnumerable<OnlinePaymentsReport.PaymentWithoutOrderRow> PaymentWithoutOrder => Report?.PaymentsWithoutOrders ?? Enumerable.Empty<OnlinePaymentsReport.PaymentWithoutOrderRow>();

		public bool CanGenerateReport
		{
			get => _canGenerateReport;
			private set => SetField(ref _canGenerateReport, value);
		}

		public bool CanCancelGeneration
		{
			get => _canCancelGeneration;
			set => SetField(ref _canCancelGeneration, value);
		}

		public bool IsReportGenerating
		{
			get => _isReportGenerating;
			private set => SetField(ref _isReportGenerating, value);
		}

		public bool CanSaveReport
		{
			get => _canSaveReport;
			private set => SetField(ref _canSaveReport, value);
		}

		public AsyncCommand GenerateReportCommand { get; }
		public DelegateCommand ExportReportCommand { get; }
		public DelegateCommand CancelGenerationCommand { get; }

		private async Task GenerateReport(CancellationToken token)
		{
			CanGenerateReport = false;
			CanCancelGeneration = true;

			try
			{
				var reportResult = await OnlinePaymentsReport.CreateAsync(
					StartDate,
					EndDate.LatestDayTime(),
					SelectedShop,
					_unitOfWork,
					token);

				_guiDispatcher.RunInGuiTread(() =>
				{
					if(reportResult.IsSuccess)
					{
						Report = reportResult.Value;
					}
					else
					{
						ShowErrors(reportResult.Errors);
					}
				});
			}
			catch(Exception ex)
			{
				
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					CanGenerateReport = true;
					CanCancelGeneration = false;
				});
			}
		}

		private void CancelGeneration()
		{
			CanCancelGeneration = false;
			GenerateReportCommand.Abort();
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

		private XLTemplate PostProcess(OnlinePaymentsReport report)
		{
			var template = report.GetRawTemplate();

			//if(!report.CarReceptionRows.Any())
			//{
			//	template.Workbook.Worksheet(1).Rows(13, 16).Delete();
			//}

			//if(!report.CarTransferRows.Any())
			//{
			//	template.Workbook.Worksheet(1).Rows(8, 11).Delete();
			//}

			return template.RenderTemplate(report);
		}

		private void ShowErrors(IEnumerable<Error> errors)
		{
			if(errors.Any())
			{
				_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				"Во время формирования отчета произошли следующие ошибки:\n" +
				"- " +
				string.Join("\n- ", errors.Select(e => e.Message)),
				"Ошибка при создании отчета");
			}
		}
	}
}
