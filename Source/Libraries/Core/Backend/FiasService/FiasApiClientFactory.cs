using System;
using Fias.Client.Cache;
using Vodovoz.Settings.Fias;

namespace Fias.Client
{
	internal class FiasApiClientFactory : IFiasApiClientFactory
	{
		private readonly IFiasApiSettings _fiasApiSettings;
		private readonly GeocoderCache _geocoderCache;

		public FiasApiClientFactory(IFiasApiSettings fiasApiSettings, GeocoderCache geocoderCache)
		{
			_fiasApiSettings = fiasApiSettings ?? throw new ArgumentNullException(nameof(fiasApiSettings));
			_geocoderCache = geocoderCache ?? throw new ArgumentNullException(nameof(geocoderCache));
		}

		public IFiasApiClient CreateClient()
		{
			return new FiasApiClient(_fiasApiSettings.FiasApiBaseUrl, _fiasApiSettings.FiasApiToken, _geocoderCache);
		}
	}
}
