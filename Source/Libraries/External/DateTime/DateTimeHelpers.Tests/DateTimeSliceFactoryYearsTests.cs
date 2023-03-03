using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateTimeHelpers.Tests
{
	public class DateTimeSliceFactoryYearsTests
	{
		[Test]
		public void CreateYearSlices_CreatesYearSlices()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;

			var slices = DateTimeSliceFactory.CreateYearsSlices(startDate, endDate);

			Assert.IsInstanceOf<IEnumerable<DateTimeSlice>>(slices);
			Assert.IsInstanceOf<IEnumerable<DateTimeYearSlice>>(slices);
		}

		[Test]
		public void CreateYearSlices_DontCreateSlicesIfEndDateIsBeforeStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue;

			Assert.Throws<ArgumentException>(() =>
				DateTimeSliceFactory.CreateYearsSlices(startDate, endDate));
		}

		[Test]
		public void CreateYearSlices_CreatesSlicesEndsAtEndDate()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateYearsSlices(startDate, endDate);

			var lastSlice = slices.LastOrDefault();

			Assert.IsNotNull(lastSlice);
			Assert.That(lastSlice.EndDate,
				Is.EqualTo(endDate));
		}

		[Test]
		public void CreateYearSlices_CreatesSlicesStartsAtStartDate()
		{
			var startDate = DateTime.MinValue.AddMinutes(10);
			var endDate = DateTime.MinValue.AddMinutes(20);

			var slices = DateTimeSliceFactory.CreateYearsSlices(startDate, endDate);

			var firstSlice = slices.FirstOrDefault();

			Assert.IsNotNull(firstSlice);
			Assert.That(firstSlice.StartDate, Is.EqualTo(startDate));
		}

		[Test]
		public void CreateYearSlices_CreatesExactlyOneIfYearIsSame()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate;

			var slices = DateTimeSliceFactory.CreateYearsSlices(startDate, endDate);

			Assert.That(slices.Count(), Is.EqualTo(1));
		}

		[Test]
		public void CreateYearsSlices_CreatesSlicesWithBorderAtMidnightOflastDayOfPreviousQuarterAndFirstOfCurrent()
		{
			var startDate = DateTime.MinValue;
			var endDate = startDate.AddYears(4);

			var slices = DateTimeSliceFactory.CreateYearsSlices(startDate, endDate);

			Assert.That(slices.Skip(1)
				.All(x => x.StartDate == x.StartDate.FirstYearDay().Date));
			Assert.That(slices.SkipLast(1)
				.All(x => x.EndDate == x.EndDate.LastYearDay().LatestDayTime()));
		}
	}
}
