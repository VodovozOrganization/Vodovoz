using ClosedXML.Report;
using NetTopologySuite.Geometries;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Extensions;
using Vodovoz.Services;
using Vodovoz.Tools.Logistic;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryPercentCoverageReport;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public class FastDeliveryPercentCoverageReportViewModel : DialogTabViewModelBase
	{
		private const int _defaultStartHour = 9;
		private const int _defaultEndHour = 18;
		private const string _templatePath = @".\Reports\Logistic\FastDeliveryPercentCoverageReport.xlsx";

		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
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
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			ITrackRepository trackRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_interactiveService = interactiveService;

			Title = "Отчет о доступности услуги \"Доставка за час\"";

			var hours = new List<TimeSpan>();

			for(TimeSpan span = TimeSpan.Zero;
				span < TimeSpan.FromHours(24);
				span = span.Add(TimeSpan.FromHours(1)))
			{
				hours.Add(span);
			}

			Hours = hours.AsEnumerable();

			StartDate = EndDate = DateTime.Today.AddDays(-1);

			DriverDisconnectedTimespan = TimeSpan.FromMinutes(-20);

			StartHour = TimeSpan.FromHours(_defaultStartHour);
			EndHour = TimeSpan.FromHours(_defaultEndHour);
		}

		public TimeSpan DriverDisconnectedTimespan { get; }

		public IEnumerable<TimeSpan> Hours { get; }

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

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		private async Task<TotalsRow> GetData(
			DateTime startDate,
			DateTime endDate,
			TimeSpan startHour,
			TimeSpan endHour)
		{
			UoW.Session.DefaultReadOnly = true;

			var requestedDateTimes = GenerateDateTimes(startDate, endDate, startHour, endHour);

			var routeListHistoryStatuses = new RouteListStatus[]
			{
				RouteListStatus.EnRoute,
				RouteListStatus.Delivered,
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

			var rawRows = new List<ValueRow>();

			foreach(var date in requestedDateTimes)
			{
				RouteList routeListAlias = null;

				var trackSubquery = QueryOver.Of<Track>()
					.Where(x => x.RouteList.Id == routeListAlias.Id)
					.Select(x => x.Id);

				var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);

				query.Where(() => routeListAlias.AdditionalLoadingDocument != null);

				query.Where(
					Restrictions.And(
						Restrictions.Or(
							Restrictions.Ge(Projections.Property(() => routeListAlias.DeliveredAt), date),
							Restrictions.IsNull(Projections.Property(() => routeListAlias.DeliveredAt))),
						Restrictions.In(Projections.Property(() => routeListAlias.Status), routeListHistoryStatuses)));

				TrackPoint trackPointAlias = null;

				trackSubquery.Inner.JoinAlias(x => x.TrackPoints, () => trackPointAlias)
					.Where(Restrictions.Le(Projections.Property(() => trackPointAlias.ReceiveTimeStamp), date))
					.Where(Restrictions.Ge(Projections.Property(() => trackPointAlias.TimeStamp), date.Add(DriverDisconnectedTimespan)))
					.Take(1);

				query.Where(Restrictions.IsNotNull(Projections.SubQuery(trackSubquery)));

				var routeListsIds = query.Select(Projections.Property(() => routeListAlias.Id)).List<int>().ToArray();

				var lastDriversCoordinates = _trackRepository.GetLastPointForRouteLists(UoW, routeListsIds, date);

				var carsCount = lastDriversCoordinates.Count;

				var serviceRadiusAtDateTime = _deliveryRulesParametersProvider.GetMaxDistanceToLatestTrackPointKmFor(date);

				var activeDistrictsAtDateTime = _scheduleRestrictionRepository.GetDistrictsWithBorderForFastDeliveryAtDateTime(UoW, date).Select(d => d.DistrictBorder);

				var percentCoverage = CalculateCoveragePercent(activeDistrictsAtDateTime, lastDriversCoordinates, serviceRadiusAtDateTime);

				rawRows.Add(new ValueRow(date, carsCount, serviceRadiusAtDateTime, percentCoverage));
			}

			var groupingDate = startDate.Date;

			var dayGroupings = new List<DayGrouping>();

			var currentDayGroupRows = new List<ValueRow>();

			foreach(var row in rawRows)
			{
				if(row.Date.Date != groupingDate)
				{
					dayGroupings.Add(new DayGrouping(groupingDate, currentDayGroupRows));
					currentDayGroupRows = new List<ValueRow>();
					groupingDate = row.Date.Date;
				}
				currentDayGroupRows.Add(row);
			}

			dayGroupings.Add(new DayGrouping(groupingDate, currentDayGroupRows));

			return await Task.FromResult(new TotalsRow(dayGroupings));
		}

		private double CalculateCoveragePercent(IEnumerable<Geometry> districtsBorders, IList<DriverPosition> lastDriversCoordinates, double serviceRadiusAtDateTime)
		{
			var geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			if(districtsBorders.Any())
			{
				Geometry allDistricts = districtsBorders.First();

				foreach(var distr in districtsBorders.Skip(1))
				{
					allDistricts = allDistricts.Union(distr);
				}

				var totalDistrictsArea = allDistricts.Area;

				var polyCircles = new List<Polygon>();

				Geometry allRadiuces = geometryFactory.CreatePolygon();

				foreach(var position in lastDriversCoordinates)
				{
					polyCircles.Add(CreateCircle(position.ToCoordinate(), serviceRadiusAtDateTime));
				}

				foreach(var circle in polyCircles)
				{
					allRadiuces = allRadiuces.Union(circle);
				}

				var difference = allDistricts.Difference(allRadiuces).Area;

				return totalDistrictsArea != 0 ? (totalDistrictsArea - difference) / totalDistrictsArea : 0;

			}
			return 0;
		}

		private Polygon CreateCircle(Coordinate center, double radius)
		{
			var twoPi = 2 * Math.PI;

			var segmentsPointsCount = 36;

			var geometryFactory = GeometryFactory.Default;

			var perimetralRingPoints = new List<Coordinate>();

			for(double radian = 0; radian < twoPi; radian += twoPi / segmentsPointsCount)
			{
				perimetralRingPoints.Add(DistanceCalculator.FindPointByDistanceAndRadians(center, radian, radius));
			}

			perimetralRingPoints.Add(DistanceCalculator.FindPointByDistanceAndRadians(center, 0, radius));

			Polygon polyCircle = geometryFactory.CreatePolygon(perimetralRingPoints.ToArray());

			return polyCircle;
		}

		private static IList<DateTime> GenerateDateTimes(DateTime startDate, DateTime endDate, TimeSpan startHour, TimeSpan endHour)
		{
			var result = new List<DateTime>();

			var requestedHours = new List<TimeSpan>();

			var requestedDates = new List<DateTime>();

			var hourTimespan = TimeSpan.FromHours(1);

			for(var date = startDate.Date; date <= endDate; date = date.AddDays(1))
			{
				requestedDates.Add(date);
			}

			if(startHour <= endHour)
			{
				for(TimeSpan span = startHour;
				span <= endHour;
				span = span.Add(hourTimespan))
				{
					requestedHours.Add(span);
				}
			}
			else
			{
				var midnightTimespan = TimeSpan.FromHours(24);
				for(TimeSpan span = startHour;
					span < midnightTimespan;
					span = span.Add(hourTimespan))
				{
					requestedHours.Add(span);
				}

				for(TimeSpan span = TimeSpan.Zero;
					span <= endHour;
					span = span.Add(hourTimespan))
				{
					requestedHours.Add(span);
				}
			}

			foreach(var date in requestedDates)
			{
				foreach(var timeSpan in requestedHours)
				{
					var dateToAdd = date.Add(timeSpan);
					if(dateToAdd > DateTime.Now)
					{
						break;
					}
					result.Add(dateToAdd);
				}
			}

			result.Sort();

			return result;
		}

		public async Task<FastDeliveryPercentCoverageReport> GenerateReport(CancellationToken cancelationToken)
		{
			try
			{
				var report = FastDeliveryPercentCoverageReport.Create(
					StartDate,
					EndDate,
					StartHour,
					EndHour,
					await GetData(StartDate,
						EndDate,
						StartHour,
						EndHour));

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
	}
}
