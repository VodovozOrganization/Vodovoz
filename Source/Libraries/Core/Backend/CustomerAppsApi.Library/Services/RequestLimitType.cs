using System.ComponentModel.DataAnnotations;

namespace CustomerAppsApi.Library.Services
{
	public enum RequestLimitType
	{
		[Display(Name = "лимит частоты запроса цен и остатков")]
		PricesAndStocksRequestFrequencyLimit,
		[Display(Name = "лимит частоты запроса номенклатур")]
		NomenclaturesRequestFrequencyLimit,
		[Display(Name = "лимит частоты запроса адресов самовывоза")]
		SelfDeliveriesAddressesRequestFrequencyLimit,
		[Display(Name = "лимит частоты запроса пакетов аренды")]
		RentPackagesRequestFrequencyLimit
	}
}
