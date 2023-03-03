using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeHelpers.Tests
{
	public class DateTimeSliceFactoryQuartersTests
	{
		[Test]
		public void CreateQuarterSlices_CreatesQuarterSlices()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;

			var slices = DateTimeSliceFactory.CreateQuartersSlices(startDate, endDate);

			Assert.IsInstanceOf<IEnumerable<DateTimeSlice>>(slices);
			Assert.IsInstanceOf<IEnumerable<DateTimeQuarterSlice>>(slices);
		}

		[Test]
		public void CreateQuarterSlices_DontCreateSlicesIfEndDateIsBeforeStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue;

			Assert.Throws<ArgumentException>(() =>
				DateTimeSliceFactory.CreateQuartersSlices(startDate, endDate));
		}

		[Test]
		public void CreateQuarterSlices_CreatesSlicesEndsAtEndDate()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateQuartersSlices(startDate, endDate);

			var lastSlice = slices.LastOrDefault();

			Assert.IsNotNull(lastSlice);
			Assert.That(lastSlice.EndDate,
				Is.EqualTo(endDate));
		}

		[Test]
		public void CreateQuarterSlices_CreatesSlicesStartsAtStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateQuartersSlices(startDate, endDate);

			var firstSlice = slices.FirstOrDefault();

			Assert.IsNotNull(firstSlice);
			Assert.That(firstSlice.StartDate, Is.EqualTo(startDate));
		}

		[Test]
		public void CreateQuarterSlices_CreatesExactlyOneIfQuarterIsSame()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate;

			var slices = DateTimeSliceFactory.CreateQuartersSlices(startDate, endDate);

			Assert.That(slices.Count(), Is.EqualTo(1));
		}

		[Test]
		public void CreateQuartersSlices_CreatesSlicesWithBorderAtMidnightOflastDayOfPreviousQuarterAndFirstOfCurrent()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate.AddQuarters(4);

			var slices = DateTimeSliceFactory.CreateQuartersSlices(startDate, endDate);

			Assert.That(slices.Skip(1)
				.All(x => x.StartDate == x.StartDate.FirstQuarterDay().Date));
			Assert.That(slices.SkipLast(1)
				.All(x => x.EndDate == x.EndDate.LastQuarterDay().LatestDayTime()));
		}
	}
}
