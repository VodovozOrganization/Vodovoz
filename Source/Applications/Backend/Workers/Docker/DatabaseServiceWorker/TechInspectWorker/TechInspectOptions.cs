using System;

namespace DatabaseServiceWorker
{
	internal sealed class TechInspectOptions
	{
		/// <summary>
		/// Интервал обновления пробега до ТО
		/// </summary>
		public TimeSpan Interval { get; set; }
	}
}
