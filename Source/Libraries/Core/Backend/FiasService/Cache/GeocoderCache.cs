using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Geocoder;

namespace Fias.Client.Cache
{
	internal class GeocoderCache
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public GeocoderCache(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public void CacheAddress(string address, decimal latitude, decimal longitude)
		{
			if(latitude == 0)
			{
				throw new ArgumentException($"'{nameof(latitude)}' cannot be zero", nameof(latitude));
			}

			if(longitude == 0)
			{
				throw new ArgumentException($"'{nameof(longitude)}' cannot be zero", nameof(longitude));
			}

			if(string.IsNullOrWhiteSpace(address))
			{
				throw new ArgumentException($"'{nameof(address)}' cannot be null or whitespace.", nameof(address));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var addressCache = new GeocoderAddressCache
				{
					Address = address,
					Latitude = latitude,
					Longitude = longitude
				};

				using(var transaction = uow.Session.BeginTransaction())
				{
					uow.Session.Save(addressCache);
					transaction.Commit();
				}
			}
		}

		public void CacheCoordinates(decimal latitude, decimal longitude, string address)
		{
			if(latitude == 0)
			{
				throw new ArgumentException($"'{nameof(latitude)}' cannot be zero", nameof(latitude));
			}

			if(longitude == 0)
			{
				throw new ArgumentException($"'{nameof(longitude)}' cannot be zero", nameof(longitude));
			}

			if(string.IsNullOrWhiteSpace(address))
			{
				throw new ArgumentException($"'{nameof(address)}' cannot be null or whitespace.", nameof(address));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var cacheId = new GeocoderCoordinatesCache { Latitude = latitude, Longitude = longitude };
				var coordinatesCache = uow.Session.Get<GeocoderCoordinatesCache>(cacheId);
				if(coordinatesCache == null)
				{
					coordinatesCache = cacheId;
				}

				coordinatesCache.Address = address;

				using(var transaction = uow.Session.BeginTransaction())
				{
					uow.Session.Save(coordinatesCache);
					transaction.Commit();
				}
			}
		}

		public GeocoderCoordinatesCache GetAddress(decimal latitude, decimal longitude)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var cacheId = new GeocoderCoordinatesCache { Latitude = latitude, Longitude = longitude };
				var coordinatesCache = uow.Session.Get<GeocoderCoordinatesCache>(cacheId);
				return coordinatesCache;
			}
		}

		public GeocoderAddressCache GetCoordinates(string address)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				GeocoderAddressCache geocoderAddressCacheAlias = null;
				var query = uow.Session.QueryOver(() => geocoderAddressCacheAlias)
					.Where(() => geocoderAddressCacheAlias.Address == address);

				var result = query.SingleOrDefault<GeocoderAddressCache>();
				return result;
			}
		}
	}
}
