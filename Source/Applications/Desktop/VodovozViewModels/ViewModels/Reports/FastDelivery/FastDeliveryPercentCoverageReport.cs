using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
