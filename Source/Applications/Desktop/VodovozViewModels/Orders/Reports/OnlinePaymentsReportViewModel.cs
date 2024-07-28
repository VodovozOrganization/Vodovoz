using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Errors;
using DateTimeHelpers;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public class OnlinePaymentsReportViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private DateTime _startDate;
		private DateTime _endDate;
		private bool _isDateTimeRangeYesterday;
		private bool _isDateTimeRangeLast3Days;
		private bool _isDateTimeRangeCustomPeriod;
		private OnlinePaymentsReport _report;
		private bool _canGenerateReport;
		private bool _isReportGenerating;

		public OnlinePaymentsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IPaymentsRepository paymentsRepository,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Title = "Отчет по оплатам OnLine заказов";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			_paymentsRepository = paymentsRepository
				?? throw new ArgumentNullException(nameof(paymentsRepository));
			_guiDispatcher = guiDispatcher
				?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));

			Shops = _paymentsRepository.GetAllShopsFromTinkoff(_unitOfWork);

			IsDateTimeRangeYesterday = true;

			GenerateReportCommand = new AsyncCommand(GenerateReport, () => CanGenerateReport);
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

		public bool CanGenerateReport
		{
			get => _canGenerateReport;
			private set => SetField(ref _canGenerateReport, value);
		}

		public bool IsReportGenerating
		{
			get => _isReportGenerating;
			private set => SetField(ref _isReportGenerating, value);
		}

		public AsyncCommand GenerateReportCommand { get; }

		public string SelectedShop { get; set; }

		private async Task GenerateReport(CancellationToken token)
		{
			CanGenerateReport = false;

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
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					CanGenerateReport = true;
				});
			}
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
