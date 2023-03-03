using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeHelpers.Tests
{
	public class DateTimeSliceFactoryMonthsTests
	{
		[Test]
		public void CreateMonthSlices_CreatesMonthSlices()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;

			var slices = DateTimeSliceFactory.CreateMonthsSlices(startDate, endDate);

			Assert.IsInstanceOf<IEnumerable<DateTimeSlice>>(slices);
			Assert.IsInstanceOf<IEnumerable<DateTimeMonthSlice>>(slices);
		}

		[Test]
		public void CreateMonthSlices_DontCreateSlicesIfEndDateIsBeforeStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue;

			Assert.Throws<ArgumentException>(() =>
				DateTimeSliceFactory.CreateMonthsSlices(startDate, endDate));
		}

		[Test]
		public void CreateMonthSlices_CreatesSlicesEndsAtEndDate()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateMonthsSlices(startDate, endDate);

			var lastSlice = slices.LastOrDefault();

			Assert.IsNotNull(lastSlice);
			Assert.That(lastSlice.EndDate,
				Is.EqualTo(endDate));
		}

		[Test]
		public void CreateMonthSlices_CreatesSlicesStartsAtStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateMonthsSlices(startDate, endDate);

			var firstSlice = slices.FirstOrDefault();

			Assert.IsNotNull(firstSlice);
			Assert.That(firstSlice.StartDate, Is.EqualTo(startDate));
		}

		[Test]
		public void CreateMonthSlices_CreatesExactlyOneIfMonthIsSame()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate;

			var slices = DateTimeSliceFactory.CreateMonthsSlices(startDate, endDate);

			Assert.That(slices.Count(), Is.EqualTo(1));
		}

		[Test]
		public void CreateMonthsSlices_CreatesSlicesWithBorderAtMidnightOflastDayOfPreviousMonthAndFirstOfCurrent()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate.AddMonths(24);

			var slices = DateTimeSliceFactory.CreateMonthsSlices(startDate, endDate);

			Assert.That(slices.Skip(1)
				.All(x => x.StartDate == x.StartDate.FirstMonthDay().Date));
			Assert.That(slices.SkipLast(1)
				.All(x => x.EndDate == x.EndDate.LastMonthDay().LatestDayTime()));
		}
	}
}
