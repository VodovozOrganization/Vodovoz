using System;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Counterparty;

namespace CustomerAppsApi.Library.Converters
{
	public class CameFromConverter : ICameFromConverter
	{
		private readonly ICounterpartySettings _counterpartySettings;

		public CameFromConverter(ICounterpartySettings counterpartySettings)
		{
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
		}
		
		public CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId)
		{
			if(cameFromId == _counterpartySettings.GetMobileAppCounterpartyCameFromId)
			{
				return CounterpartyFrom.MobileApp;
			}

			if(cameFromId == _counterpartySettings.GetWebSiteCounterpartyCameFromId)
			{
				return CounterpartyFrom.WebSite;
			}

			if(cameFromId == _counterpartySettings.GetAiBotCounterpartyCameFromId)
			{
				return CounterpartyFrom.AiBot;
			}
			
			throw new ArgumentException($"Неизвестно откуда пришел клиент Id {cameFromId}");
		}
	}
}
