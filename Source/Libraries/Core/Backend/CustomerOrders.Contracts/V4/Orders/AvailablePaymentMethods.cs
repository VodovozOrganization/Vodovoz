using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V4.Orders
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
		public IEnumerable<ExternalOrderPaymentType> OnlineOrderPaymentTypes { get; set; }
		/// <summary>
		/// Источники оплат
		/// </summary>
		[JsonPropertyName("onlinePaymentSource")]
		public IEnumerable<ExternalPaymentSource> OnlinePaymentSources { get; set; }

		public static AvailablePaymentMethods Create(ExternalSource source, ExternalOrderPaymentType onlineOrderPaymentType)
		{
			var availablePayments = new AvailablePaymentMethods();

			var onlineOrderPaymentTypes =
				Enum.GetValues(typeof(ExternalOrderPaymentType))
					.Cast<ExternalOrderPaymentType>()
					.Where(enumValue => enumValue != onlineOrderPaymentType)
					.ToList();
			
			List<ExternalPaymentSource> onlinePaymentSources;

			switch(source)
			{
				case ExternalSource.VodovozWebSite:
					onlinePaymentSources = new List<ExternalPaymentSource>(
						new[]
						{
							ExternalPaymentSource.FromVodovozWebSite,
							ExternalPaymentSource.FromVodovozWebSiteByQr,
							ExternalPaymentSource.FromVodovozWebSiteByYandexSplit
						});
					break;
				case ExternalSource.MobileApp:
					onlinePaymentSources = new List<ExternalPaymentSource>(
						new[]
						{
							ExternalPaymentSource.FromMobileApp,
							ExternalPaymentSource.FromMobileAppByQr,
							ExternalPaymentSource.FromMobileAppByYandexSplit
						});
					break;
				default:
					onlinePaymentSources = new List<ExternalPaymentSource>();
					break;
			}

			availablePayments.OnlineOrderPaymentTypes = onlineOrderPaymentTypes;
			availablePayments.OnlinePaymentSources = onlinePaymentSources;
			return availablePayments;
		}
	}
}
