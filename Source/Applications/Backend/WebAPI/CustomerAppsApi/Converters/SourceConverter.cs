using System;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Converters
{
	public class SourceConverter : ISourceConverter
	{
		public NomenclatureOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return NomenclatureOnlineParameterType.ForMobileApp;
				case Source.VodovozWebSite:
					return NomenclatureOnlineParameterType.ForVodovozWebSite;
				case Source.KulerSaleWebSite:
					return NomenclatureOnlineParameterType.ForKulerSaleWebSite;
				default:
					throw new InvalidOperationException("Неизвестный источник запроса");
			}
		}
	}
}
