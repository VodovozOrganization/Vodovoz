using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

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
	}
}
