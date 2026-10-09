using CustomerOrders.Abstractions;
using System;
using CustomerApp.Contracts.Common;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Extensions
{
	public static class SourceExtensions
	{
		public static GoodsOnlineParameterType ToGoodsOnlineParameterType(this ExternalSource source)
		{
			switch(source)
			{
				case ExternalSource.MobileApp:
					return GoodsOnlineParameterType.ForMobileApp;
				case ExternalSource.VodovozWebSite:
					return GoodsOnlineParameterType.ForVodovozWebSite;
				case ExternalSource.KulerSaleWebSite:
					return GoodsOnlineParameterType.ForKulerSaleWebSite;
				case ExternalSource.AiBot:
					return GoodsOnlineParameterType.ForAiBot;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), $"ИПЗ {source} не поддерживается");
			}
		}

		public static GoodsOnlineParameterType ToGoodsOnlineParameterType(this Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return GoodsOnlineParameterType.ForMobileApp;
				case Source.VodovozWebSite:
					return GoodsOnlineParameterType.ForVodovozWebSite;
				case Source.KulerSaleWebSite:
					return GoodsOnlineParameterType.ForKulerSaleWebSite;
				case Source.AiBot:
					return GoodsOnlineParameterType.ForAiBot;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), $"ИПЗ {source} не поддерживается");
			}
		}
		
		public static CounterpartyFrom ToCounterpartyFrom(this ExternalSource source)
		{
			switch(source)
			{
				case ExternalSource.MobileApp:
					return CounterpartyFrom.MobileApp;
				case ExternalSource.VodovozWebSite:
					return CounterpartyFrom.WebSite;
				case ExternalSource.AiBot:
					return CounterpartyFrom.AiBot;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), $"ИПЗ {source} не поддерживается");
			}
		}
	}
}
