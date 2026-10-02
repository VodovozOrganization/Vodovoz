using System;

namespace Vodovoz.Settings.Edo
{
	public interface IEdoClosedPeriodSettings
	{
		(int Day, int Month) Q1ClosingDayMonth { get; }

		(int Day, int Month) Q2ClosingDayMonth { get; }

		(int Day, int Month) Q3ClosingDayMonth { get; }

		(int Day, int Month) Q4ClosingDayMonth { get; }

		void UpdateQ1ClosingDayMonth(int day, int month);
		void UpdateQ2ClosingDayMonth(int day, int month);
		void UpdateQ3ClosingDayMonth(int day, int month);
		void UpdateQ4ClosingDayMonth(int day, int month);

		DateTime GetClosingDate(DateTime accountingDate);

		bool IsClosedPeriod(DateTime accountingDate, DateTime? today = null);
	}
}
