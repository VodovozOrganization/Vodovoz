using System;
using System.Collections.Generic;
using Vodovoz.Settings.Database.Mindbox;

namespace Vodovoz.Mindbox.Services
{

	/// <inheritdoc />
	public sealed class MindboxRetryPolicy : IMindboxRetryPolicy
	{
		private static readonly int[] _defaultSchedule = { 1, 5, 15, 60 };

		private readonly IMindboxSettings _settings;

		public MindboxRetryPolicy(IMindboxSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		/// <inheritdoc />
		public DateTime? GetNextAttemptAt(DateTime now, int attempts, DateTime retryUntil)
		{
			if(now >= retryUntil)
			{
				return null;
			}

			// Читаем расписание при каждом вызове, чтобы изменение настройки
			// применялось без перезапуска воркера.
			var schedule = ParseSchedule(_settings.RetryScheduleMinutes);

			var minutes = attempts > 0 && attempts <= schedule.Length
				? schedule[attempts - 1]
				: Math.Max(1, _settings.RetryIntervalMinutes);

			var nextAttemptAt = now.AddMinutes(minutes);

			return nextAttemptAt >= retryUntil ? (DateTime?)null : nextAttemptAt;
		}

		private static int[] ParseSchedule(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
			{
				return _defaultSchedule;
			}

			var minutes = new List<int>();

			foreach(var part in value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if(int.TryParse(part.Trim(), out var minute) && minute > 0)
				{
					minutes.Add(minute);
				}
			}

			return minutes.Count > 0
				? minutes.ToArray()
				: _defaultSchedule;
		}
	}
}
