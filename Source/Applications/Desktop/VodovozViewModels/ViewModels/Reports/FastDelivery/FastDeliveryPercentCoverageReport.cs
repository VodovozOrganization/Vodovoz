using DateTimeHelpers;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Delivery;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public class FastDeliveryPercentCoverageReport
	{
		private FastDeliveryPercentCoverageReport(
			DateTime startDate,
			DateTime endDate,
			TimeSpan startHour,
			TimeSpan endHour,
			TotalsRow grouping)
		{
			StartDate = startDate;
			EndDate = endDate;

			StartHour = startHour;
			EndHour = endHour;

			Grouping = grouping;

			CreatedAt = DateTime.Now;

			Rows = TransformToRows();
		}

		public DateTime CreatedAt { get; }

		public DateTime StartDate { get; }

		public TimeSpan StartHour { get; }

		public TimeSpan EndHour { get; }

		public DateTime EndDate { get; }

		public TotalsRow Grouping { get; }

		public IList<Row> Rows { get; }

		private IList<Row> TransformToRows()
		{
			var result = new List<Row>
			{
				Grouping,
				new Subheader()
			};

			foreach(var dayGroup in Grouping)
			{
				result.Add(dayGroup);

				foreach(var hourGroup in dayGroup)
				{
					result.Add(hourGroup);
				}

				result.Add(new EmptyRow());
			}

			return result;
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
				for(var span = startHour; span <= endHour; span = span.Add(hourTimespan))
				{
					requestedHours.Add(span);
				}
			}
			else
			{
				var midnightTimespan = TimeSpan.FromHours(24);

				for(var span = startHour; span < midnightTimespan; span = span.Add(hourTimespan))
				{
					requestedHours.Add(span);
				}

				for(var span = TimeSpan.Zero; span <= endHour; span = span.Add(hourTimespan))
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

		public static async Task<TotalsRow> GetData(
			IUnitOfWork UoW,
			DateTime startDate,
			DateTime endDate,
			TimeSpan startHour,
			TimeSpan endHour,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryRepository _deliveryRepository,
			ITrackRepository _trackRepository,
			IScheduleRestrictionRepository _scheduleRestrictionRepository,
			CancellationToken cancellationToken)
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
				if(cancellationToken.IsCancellationRequested)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}

				RouteList routeListAlias = null;

				var additionalLoadingRestriction = Restrictions.Where(() => routeListAlias.AdditionalLoadingDocument != null);

				var dateRestriction = Restrictions.And(
					Restrictions.Or(
						Restrictions.Ge(Projections.Property(() => routeListAlias.DeliveredAt), date),
						Restrictions.IsNull(Projections.Property(() => routeListAlias.DeliveredAt))),
					Restrictions.In(Projections.Property(() => routeListAlias.Status), routeListHistoryStatuses));

				TrackPoint trackPointAlias = null;

				var trackSubquery = QueryOver.Of<Track>()
					.Where(x => x.RouteList.Id == routeListAlias.Id)
				.Select(x => x.Id);

				var driverDisconnectedTimespan = TimeSpan.FromMinutes(-(int)deliveryRulesSettings.MaxTimeOffsetForLatestTrackPoint.TotalMinutes);

				trackSubquery.Inner.JoinAlias(x => x.TrackPoints, () => trackPointAlias)
					.Where(Restrictions.Le(Projections.Property(() => trackPointAlias.ReceiveTimeStamp), date))
					.Where(Restrictions.Ge(Projections.Property(() => trackPointAlias.TimeStamp), date.Add(driverDisconnectedTimespan)))
					.Take(1);

				var trackRestriction = Restrictions.IsNotNull(Projections.SubQuery(trackSubquery));

				var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);
				query.Where(additionalLoadingRestriction);
				query.Where(dateRestriction);
				query.Where(trackRestriction);

				var routeListsIds = query.Select(Projections.Property(() => routeListAlias.Id)).List<int>().ToArray();

				RouteListItem routeListItemAlias = null;
				Order orderAlias = null;

				var addressCountSubquery = QueryOver.Of(() => routeListItemAlias)
					.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.Where(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
					.And(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
					.And(() => orderAlias.IsFastDelivery)
					.And(Restrictions.Or(
						Restrictions.Ge(Projections.Property(() => orderAlias.TimeDelivered), date),
						Restrictions.Eq(Projections.Property(() => routeListItemAlias.Status), RouteListItemStatus.EnRoute)))
					.And(() => routeListItemAlias.CreationDate <= date)
					.Select(Projections.Count(() => routeListItemAlias.Id));

				var routeListMaxFastDeliveryOrdersSubquery = QueryOver.Of<RouteListMaxFastDeliveryOrders>()
					.Where(
						m => m.RouteList.Id == routeListAlias.Id
							 && m.StartDate <= date
							 && (m.EndDate == null || m.EndDate > date))
					.Select(m => m.MaxOrders)
					.OrderBy(m => m.StartDate).Desc
					.Take(1);

				var routeListMaxFastDeliveryOrdersProjection = Projections.Conditional(Restrictions.IsNull(Projections.SubQuery(routeListMaxFastDeliveryOrdersSubquery)),
					Projections.Constant(deliveryRulesSettings.MaxFastOrdersPerSpecificTime),
					Projections.SubQuery(routeListMaxFastDeliveryOrdersSubquery));

				query.Where(Restrictions.GtProperty(routeListMaxFastDeliveryOrdersProjection, Projections.SubQuery(addressCountSubquery)));

				var actualQuery = UoW.Session.QueryOver<RouteList>(() => routeListAlias);
				actualQuery.Where(additionalLoadingRestriction);
				actualQuery.Where(dateRestriction);
				actualQuery.Where(trackRestriction);
				actualQuery.Where(Restrictions.GtProperty(routeListMaxFastDeliveryOrdersProjection, Projections.SubQuery(addressCountSubquery)));

				var actualRouteListsIds = actualQuery.Select(Projections.Property(() => routeListAlias.Id)).List<int>().ToArray();

				var lastDriversCoordinates = _trackRepository.GetLastPointForRouteListsWithRadius(UoW, routeListsIds, date);
				var actualLastDriversCoordinates = _trackRepository.GetLastPointForRouteListsWithRadius(UoW, actualRouteListsIds, date);

				var carsCount = lastDriversCoordinates.Count;
				var actualCarsCount = actualLastDriversCoordinates.Count;

				var serviceRadiusAtDateTime =
					carsCount > 0
					? lastDriversCoordinates.Average(d => d.FastDeliveryRadius)
					: _deliveryRepository.GetMaxDistanceToLatestTrackPointKmFor(date);

				var actualServiceRadiusAtDateTime =
					actualCarsCount > 0
						? actualLastDriversCoordinates.Average(d => d.FastDeliveryRadius)
						: _deliveryRepository.GetMaxDistanceToLatestTrackPointKmFor(date);

				var activeDistrictsAtDateTime =
					_scheduleRestrictionRepository.GetDistrictsWithBorderForFastDeliveryAtDateTime(UoW, date).Select(d => d.DistrictBorder);

				var percentCoverage = DistanceCalculator.CalculateCoveragePercent(
					activeDistrictsAtDateTime.ToList(),
					lastDriversCoordinates);

				var actualPercentCoverage = DistanceCalculator.CalculateCoveragePercent(
					activeDistrictsAtDateTime.ToList(),
					actualLastDriversCoordinates);

				#region notDeliveredAddressesQuery

				FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistoryAlias = null;
				DeliveryPoint deliveryPointAlias = null;

				var orderSubQuery = QueryOver.Of(() => orderAlias)
					.Where(o => o.DeliveryPoint.Id == deliveryPointAlias.Id)
					.And(o => o.CreateDate.Value.Date == fastDeliveryAvailabilityHistoryAlias.VerificationDate.Date)
					.And(o => o.IsFastDelivery == false)
					.Select(o => o.Id);

				var validLastFastDeliveryCheckingSubQuery = QueryOver.Of<FastDeliveryAvailabilityHistoryItem>()
					.Where(fhi => fhi.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
					.And(fhi => fhi.IsValidToFastDelivery)
					.Select(fhi => fhi.Id);

				var deliveryPointIdsInFutureChecks = QueryOver.Of<FastDeliveryAvailabilityHistory>()
					.Where(fh => fh.DeliveryPoint.Id != null)
					.And(fh => fh.VerificationDate >= date.AddHours(1))
					.And(fh => fh.VerificationDate < date.LatestDayTime())
					.SelectList(list => list
						.SelectGroup(fh => fh.DeliveryPoint.Id));

				var lastFastDeliveryCheckingIds =
					UoW.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
						.JoinAlias(fh => fh.DeliveryPoint, () => deliveryPointAlias)
						.SelectList(list => list
							.Select(Projections.Max(() => fastDeliveryAvailabilityHistoryAlias.Id))
							.SelectGroup(() => deliveryPointAlias.Id)
						)
						.And(fh => fh.VerificationDate >= date)
						.And(fh => fh.VerificationDate < date.AddHours(1))
						.WithSubquery.WhereProperty(() => deliveryPointAlias.Id).NotIn(deliveryPointIdsInFutureChecks)
						.List<object[]>()
						.Select(x => x[0]);

				var notDeliveredAddresses =
					UoW.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
						.JoinAlias(fh => fh.DeliveryPoint, () => deliveryPointAlias)
						.WhereRestrictionOn(fh => fh.Id).IsInG(lastFastDeliveryCheckingIds)
						.WithSubquery.WhereExists(orderSubQuery)
						.WithSubquery.WhereNotExists(validLastFastDeliveryCheckingSubQuery)
						.SelectList(list => list
							.Select(() => deliveryPointAlias.Id))
						.List<int>();

				#endregion

				rawRows.Add(
					new ValueRow(
						date,
						carsCount,
						serviceRadiusAtDateTime,
						actualServiceRadiusAtDateTime,
						percentCoverage,
						actualPercentCoverage,
						notDeliveredAddresses.Count));
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

		public static FastDeliveryPercentCoverageReport Create(
			DateTime startDate,
			DateTime endDate,
			TimeSpan startHour,
			TimeSpan endHour,
			TotalsRow grouping)
		{
			if(startDate > endDate)
			{
				throw new ArgumentException("Дата окончания не может предшествовать дате начала", nameof(endDate));
			}

			return new FastDeliveryPercentCoverageReport(startDate, endDate, startHour, endHour, grouping);
		}

		public abstract class Row
		{
			protected Row() {}
			
			protected Row(
				double carsCount,
				double serviceRadius,
				double actualServiceRadius,
				double percentCoverage,
				double actualPercentCoverage,
				int notDeliveredAddresses)
			{
				CarsCount = carsCount;
				ServiceRadius = serviceRadius;
				ActualServiceRadius = actualServiceRadius;
				PercentCoverage = percentCoverage;
				ActualPercentCoverage = actualPercentCoverage;
				NotDeliveredAddresses = notDeliveredAddresses;
			}
			
			public virtual string SubHeader { get; }
			public virtual double CarsCount { get; }
			public virtual double ServiceRadius { get; }
			public virtual double ActualServiceRadius { get; }
			public virtual double PercentCoverage { get; }
			public virtual double ActualPercentCoverage { get; }
			public virtual int NotDeliveredAddresses { get; }
		}

		public class Subheader : Row
		{
			public override string SubHeader => "Детальная информация";
		}

		public class EmptyRow : Row {}

		public class TotalsRow : Row, IGrouping<bool, DayGrouping>
		{
			private readonly IEnumerable<DayGrouping> _rows;

			public TotalsRow(IEnumerable<DayGrouping> rows)
			{
				_rows = rows;
			}

			public bool Key => true;
			public override string SubHeader => string.Empty;
			public override double CarsCount => _rows.Sum(x => x.CarsCount) / _rows.Count();
			public override double ServiceRadius => _rows.Sum(x => x.ServiceRadius) / _rows.Count();
			public override double ActualServiceRadius => _rows.Sum(x => x.ActualServiceRadius) / _rows.Count();
			public override double PercentCoverage => _rows.Sum(x => x.PercentCoverage) / _rows.Count();
			public override double ActualPercentCoverage => _rows.Sum(x => x.ActualPercentCoverage) / _rows.Count();
			public override int NotDeliveredAddresses => _rows.Sum(x => x.NotDeliveredAddresses);

			#region IGrouping<bool, DayGrouping>
			public IEnumerator<DayGrouping> GetEnumerator()
			{
				return _rows.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion
		}

		public class DayGrouping : Row, IGrouping<DateTime, ValueRow>
		{
			private readonly IEnumerable<ValueRow> _rows;

			public DayGrouping(DateTime dateTime, IEnumerable<ValueRow> rows)
			{
				Key = dateTime;
				_rows = rows;
			}

			public DateTime Key { get; }

			public IEnumerable<ValueRow> Rows => _rows;

			public override string SubHeader => Key.ToString("dd.MM.yy");
			public override double CarsCount => _rows.Sum(x => x.CarsCount) / _rows.Count();
			public override double ServiceRadius => _rows.Sum(x => x.ServiceRadius) / _rows.Count();
			public override double ActualServiceRadius => _rows.Sum(x => x.ActualServiceRadius) / _rows.Count();
			public override double PercentCoverage => _rows.Sum(x => x.PercentCoverage) / _rows.Count();
			public override double ActualPercentCoverage => _rows.Sum(x => x.ActualPercentCoverage) / _rows.Count();
			public override int NotDeliveredAddresses => _rows.Sum(x => x.NotDeliveredAddresses);

			#region IGrouping<DateTime, FastDeliveryPercentCoverageReportValueRow>
			public IEnumerator<ValueRow> GetEnumerator()
			{
				return _rows.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion
		}

		public class ValueRow : Row
		{
			private DateTime _dateTime;

			public ValueRow(
				DateTime dateTime,
				double carsCount,
				double serviceRadius,
				double actualServiceRadius,
				double percentCoverage,
				double actualPercentCoverage,
				int notDeliveredAddresses)
				: base(carsCount, serviceRadius, actualServiceRadius, percentCoverage, actualPercentCoverage, notDeliveredAddresses)
			{
				_dateTime = dateTime;
			}

			public DateTime Date => _dateTime;

			public override string SubHeader => $"{HourSpan:hh}-00";

			private TimeSpan HourSpan => _dateTime - _dateTime.Date;
		}
	}
}
