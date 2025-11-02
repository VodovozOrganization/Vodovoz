using System;

namespace Vodovoz.SmsInformerWorker.Options
{
	public class SmsInformerOptions
	{
		/// <summary>
		/// Интервал проверки в базе неотправленных СМС
		/// </summary>
		public TimeSpan SmsScanInterval { get; set; }
	}
}
