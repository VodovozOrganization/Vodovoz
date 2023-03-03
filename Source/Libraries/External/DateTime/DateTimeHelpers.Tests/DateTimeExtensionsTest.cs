using NUnit.Framework;
using System;

namespace DateTimeHelpers.Tests
{
	public class DateTimeExtensionsTest
	{
		public static (DateTime dateTime, int weekNumber)[] GetWeekNumberCases = new
		{
			(new DateTime(1,1,1), 1),
			(new DateTime(1, 1, 1), 1)
		};

		[TestCaseSource(nameof(GetWeekNumberCases))]
		public void DateTimeExtensionGetWeekNumber_RealDatesTest(
			DateTime dateTime, int weekNumber)
		{
			Assert.That(dateTime.GetWeekNumber(), Is.EqualTo(weekNumber));
		}
	}
}
