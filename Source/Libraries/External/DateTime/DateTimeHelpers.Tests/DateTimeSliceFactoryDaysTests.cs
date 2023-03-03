using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeHelpers.Tests
{
	public class DateTimeSliceFactoryDaysTests
	{


		[Test]
		public void CreateDaysSlices_CreatesDaysSlices()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;

			var slices = DateTimeSliceFactory.CreateDaysSlices(startDate, endDate);

			Assert.IsInstanceOf<IEnumerable<DateTimeSlice>>(slices);
			Assert.IsInstanceOf<IEnumerable<DateTimeDaySlice>>(slices);
		}


		[Test]
		public void CreateDaysSlices_DontCreateSlicesIfEndDateIsBeforeStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue;

			Assert.Throws<ArgumentException>(() =>
				DateTimeSliceFactory.CreateDaysSlices(startDate, endDate));
		}

		[Test]
		public void CreateDaysSlices_CreatesSlicesEndsAtEndDate()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue.AddMinutes(10);

			var slices = DateTimeSliceFactory.CreateDaysSlices(startDate, endDate);

			var lastSlice = slices.LastOrDefault();

			Assert.IsNotNull(lastSlice);
			Assert.That(lastSlice.EndDate,
				Is.EqualTo(endDate));
		}

		[Test]
		public void CreateDaysSlices_CreatesSlicesStartsAtStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateDaysSlices(startDate, endDate);

			var firstSlice = slices.FirstOrDefault();

			Assert.IsNotNull(firstSlice);
			Assert.That(firstSlice.StartDate, Is.EqualTo(startDate));
		}

		[Test]
		public void CreateDaysSlices_CreatesExactlyOneIfDayIsSame()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate;

			var slices = DateTimeSliceFactory.CreateDaysSlices(startDate, endDate);

			Assert.That(slices.Count(), Is.EqualTo(1));
		}

		[Test]
		public void CreateDaysSlices_CreatesSlicesWithBorderAtMidnight()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate.AddDays(30);

			var slices = DateTimeSliceFactory.CreateDaysSlices(startDate, endDate);

			Assert.That(slices.Skip(1).All(x => x.StartDate == x.StartDate.Date));
			Assert.That(slices.SkipLast(1).All(x => x.EndDate == x.EndDate.LatestDayTime()));
		}
	}
}
