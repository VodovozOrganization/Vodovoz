﻿using System;
using Vodovoz.Domain.Client;
using Vodovoz.Parameters;

namespace CustomerAppsApi.Converters
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
			return cameFromId == _counterpartySettings.GetMobileAppCounterpartyCameFromId
				? CounterpartyFrom.MobileApp
				: CounterpartyFrom.WebSite;
		}
	}
}
