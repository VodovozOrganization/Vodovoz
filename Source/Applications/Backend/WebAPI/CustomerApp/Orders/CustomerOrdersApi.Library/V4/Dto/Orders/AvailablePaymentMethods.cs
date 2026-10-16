using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Данные по доступным типам оплат
	/// </summary>
	public class AvailablePaymentMethods
	{
		/// <summary>
		/// Типы оплат
		/// </summary>
		[JsonPropertyName("onlineOrderPaymentType")]
		public IEnumerable<OnlineOrderPaymentType> OnlineOrderPaymentTypes { get; set; }
		/// <summary>
		/// Источники оплат
		/// </summary>
		[JsonPropertyName("onlinePaymentSource")]
		public IEnumerable<OnlinePaymentSource> OnlinePaymentSources { get; set; }

		public static AvailablePaymentMethods Create(Source source, OnlineOrderPaymentType onlineOrderPaymentType)
		{
			var availablePayments = new AvailablePaymentMethods();

			var onlineOrderPaymentTypes =
				Enum.GetValues<OnlineOrderPaymentType>()
					.Where(enumValue => enumValue != onlineOrderPaymentType)
					.ToList();
			
			List<OnlinePaymentSource> onlinePaymentSources;

			switch(source)
			{
				case Source.VodovozWebSite:
					onlinePaymentSources = new List<OnlinePaymentSource>(
						new[]
						{
							OnlinePaymentSource.FromVodovozWebSite,
							OnlinePaymentSource.FromVodovozWebSiteByQr,
							OnlinePaymentSource.FromVodovozWebSiteByYandexSplit
						});
					break;
				case Source.MobileApp:
					onlinePaymentSources = new List<OnlinePaymentSource>(
						new[]
						{
							OnlinePaymentSource.FromMobileApp,
							OnlinePaymentSource.FromMobileAppByQr,
							OnlinePaymentSource.FromMobileAppByYandexSplit
						});
					break;
				default:
					onlinePaymentSources = new List<OnlinePaymentSource>();
					break;
			}

			availablePayments.OnlineOrderPaymentTypes = onlineOrderPaymentTypes;
			availablePayments.OnlinePaymentSources = onlinePaymentSources;
			return availablePayments;
		}
	}
}
