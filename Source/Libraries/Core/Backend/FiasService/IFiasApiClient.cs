using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;

namespace Fias.Client
{
	public interface IFiasApiClient
	{
		IEnumerable<CityDTO> GetCitiesByCriteria(string searchString, int limit, bool isActive = true);
		IEnumerable<StreetDTO> GetStreetsByCriteria(Guid cityGuid, string searchString, int limit, bool isActive = true);
		IEnumerable<HouseDTO> GetHousesFromStreetByCriteria(Guid streetGuid, string streetDistrict, int? limit = 200, bool isActive = true);
		IEnumerable<HouseDTO> GetHousesFromCityByCriteria(Guid cityGuid, string searchString, int? limit = 200, bool isActive = true);
		Task<PointDTO> GetCoordinatesByGeoCoder(string address, CancellationToken cancellationToken);
		Task<string> GetAddressByGeoCoder(decimal latitude, decimal longitude, CancellationToken cancellationToken);
	}
}
