using CustomerOrders.Abstractions;
using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V7.Extensions
{
	public static class ExternalSourceExtensions
	{
		public static Source ToSource(this ExternalSource source)
		{
			switch(source)
			{
				case ExternalSource.VodovozWebSite:
					return Source.VodovozWebSite;
				case ExternalSource.MobileApp:
					return Source.MobileApp;
				case ExternalSource.KulerSaleWebSite:
					return Source.KulerSaleWebSite;
				case ExternalSource.AiBot:
					return Source.AiBot;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), "Неизвестный источник принятия заказа ИПЗ");
			}
		}
	}
}
