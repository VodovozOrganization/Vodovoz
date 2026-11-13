namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Ограничения поиска контрагента
	/// </summary>
	public static class CounterpartyIdentificationOptions
	{
		public const int MaxSearchResults = 50;
		public const int MaxAddressCandidates = 2000;
		public const int MaxOrders = 100;
		public const int MaxDeliveryPointsPerCounterparty = 10;
		public const int MaxPhonesPerCounterparty = 10;
	}
}
