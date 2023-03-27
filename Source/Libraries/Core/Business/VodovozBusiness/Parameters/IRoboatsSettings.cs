namespace Vodovoz.Parameters
{
	public interface IRoboatsSettings
	{
		int DefaultCounterpartyNameId { get; }
		int DefaultCounterpartyPatronymicId { get; }
		string DeliverySchedulesAudiofilesFolder { get; }
		string AddressesAudiofilesFolder { get; }
		string WaterTypesAudiofilesFolder { get; }
		string CounterpartyNameAudiofilesFolder { get; }
		string CounterpartyPatronymicAudiofilesFolder { get; }
		int MaxBanknoteForReturn { get; }
		int CallRegistryAutoRefreshInterval { get; }
		int OrdersInMonths { get; }
		bool RoboatsEnabled { get; set; }

		/// <summary>
		/// Время в течении которого звонок является активным, 
		/// по истечении которого звонок будет считаться устаревшим 
		/// и будет закрыть при следующей проверке устаревших звонков
		/// </summary>
		int CallTimeout { get; }

		/// <summary>
		/// Интервал проверки устаревших звонков (в минутах)
		/// </summary>
		int StaleCallCheckInterval { get; }
	}
}
