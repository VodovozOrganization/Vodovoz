using System;

namespace RoboatsService.Options
{
	/// <summary>
	/// Настройки робоатс
	/// </summary>
	public class RoboAtsOptions
	{
		/// <summary>
		/// Таймаут звонка курьекру
		/// </summary>
		public TimeSpan CallToCourierTimeOut { get; set; }
	}
}
