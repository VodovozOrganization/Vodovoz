using ClosedXML.Report;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Delivery;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryPercentCoverageReport;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public class FastDeliveryPercentCoverageReportViewModel : DialogTabViewModelBase
	{
		private const int _defaultStartHour = 9;
		private const int _defaultEndHour = 18;
		private const string _templatePath = @".\Reports\Logistic\FastDeliveryPercentCoverageReport.xlsx";

		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IInteractiveService _interactiveService;

		private DateTime _startDate;
		private DateTime _endDate;
		private TimeSpan _startHour;
		private TimeSpan _endHour;
		private FastDeliveryPercentCoverageReport _report;
		private bool _isSaving = false;
		private bool _canSave;
		private bool _canCancelGenerate;
		private bool _isGenerating;
		private IEnumerable<string> _lastGenerationErrors;

		public FastDeliveryPercentCoverageReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryRepository deliveryRepository,
			ITrackRepository trackRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_deliveryRulesSettings =
				deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_scheduleRestrictionRepository =
				scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_interactiveService = interactiveService;

			Title = "Отчет о доступности услуги \"Доставка за час\"";

			Initialize();
		}

		public IEnumerable<TimeSpan> Hours { get; private set; }

		public FastDeliveryPercentCoverageReport Report
		{
			get => _report;
			set
			{
				SetField(ref _report, value);
				CanSave = _report != null;
			}
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

		public TimeSpan StartHour
		{
			get => _startHour;
			set => SetField(ref _startHour, value);
		}

		public TimeSpan EndHour
		{
			get => _endHour;
			set => SetField(ref _endHour, value);
		}

		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving;
			}
		}

		public bool CanGenerate => !IsGenerating;

		public bool CanCancelGenerate
		{
			get => _canCancelGenerate;
			set => SetField(ref _canCancelGenerate, value);
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set
			{
				SetField(ref _isGenerating, value);
				OnPropertyChanged(nameof(CanGenerate));
				CanCancelGenerate = IsGenerating;
			}
		}

		public IEnumerable<string> LastGenerationErrors
		{
			get => _lastGenerationErrors;
			set => SetField(ref _lastGenerationErrors, value);
		}

		public CancellationTokenSource ReportGenerationCancellationTokenSource { get; set; }

		public async Task<FastDeliveryPercentCoverageReport> GenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				var report = Create(
					StartDate,
					EndDate,
					StartHour,
					EndHour,
					await GetData(
						UoW,
						StartDate,
						EndDate,
						StartHour,
						EndHour,
						_deliveryRulesSettings,
						_deliveryRepository,
						_trackRepository,
						_scheduleRestrictionRepository,
						cancellationToken));

				return report;
			}
			finally
			{
				UoW.Session.Clear();
			}
		}

		public void ExportReport(string path)
		{
			string templatePath = _templatePath;

			var template = new XLTemplate(templatePath);

			template.AddVariable(Report);
			template.Generate();

			template.SaveAs(path);
		}

		public void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}
		
		private void Initialize()
		{
			var hours = new List<TimeSpan>();

			for(var span = TimeSpan.Zero; span < TimeSpan.FromHours(24); span = span.Add(TimeSpan.FromHours(1)))
			{
				hours.Add(span);
			}

			Hours = hours.AsEnumerable();

			StartDate = EndDate = DateTime.Today.AddDays(-1);

			StartHour = TimeSpan.FromHours(_defaultStartHour);
			EndHour = TimeSpan.FromHours(_defaultEndHour);
		}			
	}
}
