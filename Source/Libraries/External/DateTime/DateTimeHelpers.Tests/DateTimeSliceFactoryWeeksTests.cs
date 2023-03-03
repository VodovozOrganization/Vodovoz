using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeHelpers.Tests
{
	public class DateTimeSliceFactoryWeeksTests
	{
		[Test]
		public void CreateWeekSlices_CreatesWeekSlices()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;

			var slices = DateTimeSliceFactory.CreateWeeksSlices(startDate, endDate);

			Assert.IsInstanceOf<IEnumerable<DateTimeSlice>>(slices);
			Assert.IsInstanceOf<IEnumerable<DateTimeWeekSlice>>(slices);
		}

		[Test]
		public void CreateWeekSlices_DontCreateSlicesIfEndDateIsBeforeStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue;

			Assert.Throws<ArgumentException>(() =>
				DateTimeSliceFactory.CreateWeeksSlices(startDate, endDate));
		}

		[Test]
		public void CreateWeekSlices_CreatesSlicesEndsAtEndDate()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateWeeksSlices(startDate, endDate);

			var lastSlice = slices.LastOrDefault();

			Assert.IsNotNull(lastSlice);
			Assert.That(lastSlice.EndDate,
				Is.EqualTo(endDate));
		}

		[Test]
		public void CreateWeekSlices_CreatesSlicesStartsAtStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateWeeksSlices(startDate, endDate);

			var firstSlice = slices.FirstOrDefault();

			Assert.IsNotNull(firstSlice);
			Assert.That(firstSlice.StartDate, Is.EqualTo(startDate));
		}

		[Test]
		public void CreateWeekSlices_CreatesExactlyOneIfWeekIsSame()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate;

			var slices = DateTimeSliceFactory.CreateWeeksSlices(startDate, endDate);

			Assert.That(slices.Count(), Is.EqualTo(1));
		}

		[Test]
		public void CreateWeeksSlices_CreatesSlicesWithBorderAtMidnightFromMondayToSunday()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate.AddWeeks(30);

			var slices = DateTimeSliceFactory.CreateWeeksSlices(startDate, endDate);

			Assert.That(slices.Skip(1)
				.All(x => x.StartDate == x.StartDate.FirstWeekDay().Date));
			Assert.That(slices.SkipLast(1)
				.All(x => x.EndDate == x.EndDate.LastWeekDay().LatestDayTime()));
		}
	}
}
