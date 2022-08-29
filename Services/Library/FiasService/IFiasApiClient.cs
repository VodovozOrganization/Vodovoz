using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;

namespace Fias.Service
{
	public interface IFiasApiClient
	{
		IEnumerable<CityDTO> GetCitiesByCriteria(string searchString, int limit, bool isActive = true);
		IEnumerable<StreetDTO> GetStreetsByCriteria(Guid cityGuid, string searchString, int limit, bool isActive = true);
		IEnumerable<HouseDTO> GetHousesFromStreetByCriteria(Guid streetGuid, string streetDistrict, int? limit = 200, bool isActive = true);
		IEnumerable<HouseDTO> GetHousesFromCityByCriteria(Guid cityGuid, string searchString, int? limit = 200, bool isActive = true);
		PointDTO GetCoordinatesByGeoCoder(string address, CancellationToken cancellationToken);
		string GetAddressByGeoCoder(decimal latitude, decimal longitude, CancellationToken cancellationToken);
	}
}
