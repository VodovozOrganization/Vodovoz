using System;
using CustomerOrders.Contracts;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace VodovozBusiness.Extensions
{
	public static class SourceExtensions
	{
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
		
		public static Source ToSource(this ExternalSource source)
		{
			switch(source)
			{
				case ExternalSource.MobileApp:
					return Source.MobileApp;
				case ExternalSource.VodovozWebSite:
					return Source.VodovozWebSite;
				case ExternalSource.KulerSaleWebSite:
					return Source.KulerSaleWebSite;
				case ExternalSource.AiBot:
					return Source.AiBot;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение внешнего источника");
			}
		}
		
		public static ExternalSource ToExternalSource(this Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return ExternalSource.MobileApp;
				case Source.VodovozWebSite:
					return ExternalSource.VodovozWebSite;
				case Source.KulerSaleWebSite:
					return ExternalSource.KulerSaleWebSite;
				case Source.AiBot:
					return ExternalSource.AiBot;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение внешнего источника");
			}
		}
	}
}
