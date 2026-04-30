using System;
using CustomerOrders.Contracts;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace VodovozBusiness.Extensions
{
	public static class NomenclatureOnlineMarkerExtensions
	{
		public static ExternalProductMarker? ToExternalProductMarker(this NomenclatureOnlineMarker? source)
		{
			switch(source)
			{
				case null:
					return null;
				case NomenclatureOnlineMarker.ProductOfWeek:
					return ExternalProductMarker.ProductOfWeek;
				case NomenclatureOnlineMarker.Sale:
					return ExternalProductMarker.Sale;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение акции в ИПЗ");
			}
		}
	}
}
