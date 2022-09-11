using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Geocoder;

namespace Fias.Service.Cache
{
	public class GeocoderCache
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

				uow.Session.BeginTransaction();
				uow.Session.Save(addressCache);
				uow.Session.Transaction.Commit();
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
				var coordinatesCache = new GeocoderCoordinatesCache
				{
					Latitude = latitude,
					Longitude = longitude,
					Address = address
				};

				uow.Session.BeginTransaction();
				uow.Session.Save(coordinatesCache);
				uow.Session.Transaction.Commit();
			}
		}

		public GeocoderAddressCache GetAddress(decimal latitude, decimal longitude)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				GeocoderAddressCache geocoderAddressCacheAlias = null;
				var query = uow.Session.QueryOver(() => geocoderAddressCacheAlias)
					.Where(() => geocoderAddressCacheAlias.Latitude == latitude)
					.Where(() => geocoderAddressCacheAlias.Longitude == longitude);

				var result = query.SingleOrDefault<GeocoderAddressCache>();
				return result;
			}
		}

		public GeocoderCoordinatesCache GetCoordinates(string address)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				GeocoderCoordinatesCache geocoderCoordinatesCacheAlias = null;
				var query = uow.Session.QueryOver(() => geocoderCoordinatesCacheAlias)
					.Where(() => geocoderCoordinatesCacheAlias.Address == address);

				var result = query.SingleOrDefault<GeocoderCoordinatesCache>();
				return result;
			}
		}
	}
}
