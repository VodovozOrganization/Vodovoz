using System.ComponentModel.DataAnnotations;

namespace CustomerAppsApi.Library.V3.Services
{
	public enum RequestLimitType
	{
		[Display(Name = "Р»РёРјРёС‚ С‡Р°СЃС‚РѕС‚С‹ Р·Р°РїСЂРѕСЃР° С†РµРЅ Рё РѕСЃС‚Р°С‚РєРѕРІ")]
		PricesAndStocksRequestFrequencyLimit,
		[Display(Name = "Р»РёРјРёС‚ С‡Р°СЃС‚РѕС‚С‹ Р·Р°РїСЂРѕСЃР° РЅРѕРјРµРЅРєР»Р°С‚СѓСЂ")]
		NomenclaturesRequestFrequencyLimit,
		[Display(Name = "Р»РёРјРёС‚ С‡Р°СЃС‚РѕС‚С‹ Р·Р°РїСЂРѕСЃР° Р°РґСЂРµСЃРѕРІ СЃР°РјРѕРІС‹РІРѕР·Р°")]
		SelfDeliveriesAddressesRequestFrequencyLimit,
		[Display(Name = "Р»РёРјРёС‚ С‡Р°СЃС‚РѕС‚С‹ Р·Р°РїСЂРѕСЃР° РїР°РєРµС‚РѕРІ Р°СЂРµРЅРґС‹")]
		RentPackagesRequestFrequencyLimit
	}
}
