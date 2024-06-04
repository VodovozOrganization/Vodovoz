namespace Vodovoz.Settings.Car
{
	public interface ICarInsuranceSettings
	{
		/// <summary>
		/// Уведомлять о приближающемся окончании страховки ОСАГО за дней
		/// </summary>
		int OsagoEndingNotifyDaysBefore { get; }

		/// <summary>
		/// Уведомлять о приближающемся окончании страховки КАСКО за дней
		/// </summary>
		int KaskoEndingNotifyDaysBefore { get; }

		/// <summary>
		/// Обновить значение дней, за которые будет появляться уведомление об окончании ОСАГО
		/// </summary>
		/// <param name="value"></param>
		void SetOsagoEndingNotifyDaysBefore(int value);


		/// <summary>
		/// Обновить значение дней, за которые будет появляться уведомление об окончании КАСКО
		/// </summary>
		/// <param name="value"></param>
		void SetKaskoEndingNotifyDaysBefore(int value);
	}
}
