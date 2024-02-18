using System;
using Fias.Client.Cache;
using Vodovoz.Settings.Fias;

namespace Fias.Client
{
	internal class FiasApiClientFactory : IFiasApiClientFactory
	{
		private readonly IFiasApiSettings _fiasApiParametersProvider;
		private readonly GeocoderCache _geocoderCache;

		public FiasApiClientFactory(IFiasApiSettings fiasApiParametersProvider, GeocoderCache geocoderCache)
		{
			_fiasApiParametersProvider = fiasApiParametersProvider ?? throw new ArgumentNullException(nameof(fiasApiParametersProvider));
			_geocoderCache = geocoderCache ?? throw new ArgumentNullException(nameof(geocoderCache));
		}

		public IFiasApiClient CreateClient()
		{
			return new FiasApiClient(_fiasApiParametersProvider.FiasApiBaseUrl, _fiasApiParametersProvider.FiasApiToken, _geocoderCache);
		}
	}
}
