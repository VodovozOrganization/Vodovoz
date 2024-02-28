namespace Vodovoz.Settings.Roboats
{
	public interface IRoboatsSettings
	{
		string AddressesAudiofilesFolder { get; }
		int CallRegistryAutoRefreshInterval { get; }
		int CallTimeout { get; }
		string CounterpartyNameAudiofilesFolder { get; }
		string CounterpartyPatronymicAudiofilesFolder { get; }
		int DefaultCounterpartyNameId { get; }
		int DefaultCounterpartyPatronymicId { get; }
		string DeliverySchedulesAudiofilesFolder { get; }
		int MaxBanknoteForReturn { get; }
		int OrdersInMonths { get; }
		bool RoboatsEnabled { get; set; }
		int StaleCallCheckInterval { get; }
		string WaterTypesAudiofilesFolder { get; }
	}
}