﻿using System;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

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
	}
}
