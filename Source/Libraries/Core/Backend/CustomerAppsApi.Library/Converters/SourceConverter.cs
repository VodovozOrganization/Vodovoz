using System;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Converters
{
	public class SourceConverter : ISourceConverter
	{
		public GoodsOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return GoodsOnlineParameterType.ForMobileApp;
				case Source.VodovozWebSite:
					return GoodsOnlineParameterType.ForVodovozWebSite;
				case Source.KulerSaleWebSite:
					return GoodsOnlineParameterType.ForKulerSaleWebSite;
				default:
					throw new InvalidOperationException("Неизвестный источник запроса");
			}
		}
		
		public CounterpartyFrom ConvertToCounterpartyFrom(Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return CounterpartyFrom.MobileApp;
				case Source.VodovozWebSite:
					return CounterpartyFrom.WebSite;
				default:
					throw new InvalidOperationException("Неизвестный источник запроса");
			}
		}
	}
}
