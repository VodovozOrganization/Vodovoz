using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
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
			return cameFromId == _counterpartySettings.GetMobileAppCounterpartyCameFromId
				? CounterpartyFrom.MobileApp
				: CounterpartyFrom.WebSite;
		}
		
		public Result<CounterpartyFrom> ConvertSourceToCounterpartyFrom(Source source)
		{
			if(source == Source.KulerSaleWebSite)
			{
				return Result.Failure<CounterpartyFrom>(new Error("500", "Нет реализации для Кулер Сэйл"));
			}

			Enum.TryParse(((int)source).ToString(), out CounterpartyFrom result);
			
			return Result.Success(result);
		}
	}
}
