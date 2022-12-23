using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public class TurnoverWithDynamicsReport
		{
			private TurnoverWithDynamicsReport(
				DateTime startDate,
				DateTime endDate,
				string slicingType)
			{
				StartDate = startDate;
				EndDate = endDate;
				SlicingType = slicingType;
				Slices = MakeSlices(startDate, endDate, slicingType);
			}

			private IEnumerable<Slice> MakeSlices(
				DateTime startDate,
				DateTime endDate,
				string slicingType)
			{
				if(slicingType == SliceValues.Day)
				{
					return MakeDaySlices(startDate, endDate);
				}

				if(slicingType == SliceValues.Week)
				{
					return MakeWeekSlices(startDate, endDate);
				}

				if(slicingType == SliceValues.Month)
				{
					return MakeMonthSlices(startDate, endDate);
				}

				if(slicingType == SliceValues.Quarter)
				{
					return MakeQuarterSlices(startDate, endDate);
				}

				if(slicingType == SliceValues.Year)
				{
					return MakeYearSlices(startDate, endDate);
				}

				throw new ArgumentOutOfRangeException(nameof(slicingType),
					"Can't make this type of slicing");
			}

			private IEnumerable<Slice> MakeYearSlices(DateTime startDate, DateTime endDate)
			{
				var slices = new List<Slice>();

				if(startDate.Year == endDate.Year)
				{
					slices.Add(new Slice(
						startDate.ToString("yyyy"),
						startDate.Date,
						endDate.Date.AddDays(1).AddMilliseconds(-1)));

					return slices.AsEnumerable();
				}

				var dateTime = new DateTime(startDate.Year + 1, startDate.Month, 1);

				slices.Add(new Slice(
					startDate.ToString("yyyy"),
					startDate.Date,
					dateTime.AddMilliseconds(-1)));

				for(; dateTime < endDate; dateTime = dateTime.AddYears(1))
				{
					slices.Add(new Slice(
									dateTime.ToString("yyyy"),
									dateTime.Date,
									dateTime.AddYears(1).AddMilliseconds(-1)));
				}

				if(dateTime.Year == dateTime.AddDays(1).Year)
				{
					slices.Add(new Slice(
						dateTime.ToString("yyyy"),
						dateTime.Date,
						endDate.AddDays(1).AddMilliseconds(-1)));
				}

				return slices.AsEnumerable();
			}

			private IEnumerable<Slice> MakeQuarterSlices(DateTime startDate, DateTime endDate)
			{
				var slices = new List<Slice>();

				if(startDate.Year == endDate.Year && (startDate.Month / 4 + 1) == (endDate.Month / 4 + 1))
				{
					slices.Add(new Slice(
						$"{startDate.Month / 4 + 1}кв{startDate:yy}",
						startDate.Date,
						endDate.Date.AddDays(1).AddMilliseconds(-1)));
					return slices.AsEnumerable();
				}

				for(var dateTime = startDate.Date; dateTime < endDate; dateTime = dateTime.AddMonths(4))
				{
					slices.Add(new Slice(
						$"{dateTime.Month / 4 + 1}кв{dateTime:yy}",
						dateTime.Date,
						dateTime.Date.AddMonths(4).AddDays(1).AddMilliseconds(-1)));
				}

				return slices.AsEnumerable();
			}

			private IEnumerable<Slice> MakeMonthSlices(DateTime startDate, DateTime endDate)
			{
				var slices = new List<Slice>();

				if(startDate.Year == endDate.Year && startDate.Month == endDate.Month)
				{
					slices.Add(
					new Slice(
							startDate.Date.ToString("MMM.yy"),
							startDate.Date,
							endDate.Date.AddDays(1).AddMilliseconds(-1)));

					return slices.AsEnumerable();
				}

				slices.Add(new Slice(
					startDate.Date.ToString("MMM.yy"),
					startDate.Date,
					startDate.Date.AddMonths(1).AddMilliseconds(-1)));

				var dateTime = startDate.Date.AddMonths(1);
				for(; dateTime < endDate; dateTime = dateTime.AddMonths(1))
				{
					slices.Add(new Slice(
						dateTime.Date.ToString("MMM.yy"),
						dateTime.Date,
						dateTime.Date.AddMonths(1).AddMilliseconds(-1)));
				}

				if(endDate.Date.Month == endDate.Date.AddDays(1).Month)
				{
					slices.Add(new Slice(
						startDate.Date.ToString("MMM.yy"),
						dateTime.Date.AddMonths(-1),
						endDate.Date.AddDays(1).AddMilliseconds(-1)));
				}

				return slices.AsEnumerable();
			}

			private IEnumerable<Slice> MakeWeekSlices(DateTime startDate, DateTime endDate)
			{
				var slices = new List<Slice>();

				var startYear1stJan = new DateTime(startDate.Year, 1, 1);

				var startWeekNumber = (startDate - startYear1stJan).TotalDays / 7 + 1;

				var endYear1stJan = new DateTime(endDate.Year, 1, 1);

				var endWeekNumber = (endDate - endYear1stJan).TotalDays / 7 + 1;

				if(startDate.Year == endDate.Year && startWeekNumber == endWeekNumber)
				{
					slices.Add(new Slice(
							$"{startWeekNumber}нед{startDate:yy}",
							startDate,
							endDate.AddDays(1).AddMilliseconds(-1)));

					return slices.AsEnumerable();
				}

				var currentWeekNumber = startWeekNumber + 1;

				slices.Add(new Slice(
					$"{startWeekNumber}нед{startDate:yy}",
					startDate,
					startYear1stJan.AddDays(7 * (currentWeekNumber - 1) + 1).AddMilliseconds(-1)));

				for(;startYear1stJan.AddDays(7 * (currentWeekNumber - 1)) < endDate; currentWeekNumber++)
				{
					slices.Add(new Slice(
						$"{currentWeekNumber}нед{startDate:yy}",
						startYear1stJan.AddDays(7 * (currentWeekNumber - 1)),
						startYear1stJan.AddDays(7 * currentWeekNumber + 1).AddMilliseconds(-1)));
				}

				if(endDate.DayOfWeek != DayOfWeek.Sunday)
				{
					slices.Add(new Slice(
						$"{currentWeekNumber}нед{startDate:yy}",
						startYear1stJan.AddDays(7 * (currentWeekNumber - 1)),
						endDate.AddDays(1).AddMilliseconds(-1)));
				}

				return slices.AsEnumerable();
			}

			private IEnumerable<Slice> MakeDaySlices(DateTime startDate, DateTime endDate)
			{
				var slices = new List<Slice>();

				for(var dateTime = startDate.Date; dateTime <= endDate; dateTime = dateTime.AddDays(1))
				{
					slices.Add(
						new Slice(
							dateTime.ToString("dd.MM.yy"),
							dateTime.Date,
							dateTime.Date.AddDays(1).AddMilliseconds(-1)));
				}

				return slices.AsEnumerable();
			}

			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public string SlicingType { get; }

			public DateTime CreatedAt { get; }

			public IEnumerable<Slice> Slices { get; }

			public static TurnoverWithDynamicsReport Make(
				DateTime startDate,
				DateTime endDate,
				string slicingType)
			{
				return new TurnoverWithDynamicsReport(
							startDate,
							endDate,
							slicingType);
			}
		}
	}
}
