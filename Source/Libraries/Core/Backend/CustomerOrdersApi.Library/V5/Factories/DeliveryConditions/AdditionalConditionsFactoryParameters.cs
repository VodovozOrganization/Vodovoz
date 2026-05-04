namespace CustomerOrdersApi.Library.V5.Factories.DeliveryConditions
{
	/// <summary>
	/// Параметры для получения доп условий
	/// </summary>
	public class AdditionalConditionsFactoryParameters
	{
		/// <summary>
		/// Тип клиента
		/// </summary>
		public ClientType ClientTypeParameter { get; private set; }
		
		public static AdditionalConditionsFactoryParameters Create(ClientType clientType)
		{
			return new AdditionalConditionsFactoryParameters
			{
				ClientTypeParameter = clientType
			};
		}
	}
}
