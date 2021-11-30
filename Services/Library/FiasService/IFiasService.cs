using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fias.Search.DTO;

namespace Fias.Service
{
	public interface IFiasService
	{
		IEnumerable<CityDTO> GetCitiesByCriteria(string searchString, int limit, bool isActive = true);
		IEnumerable<CityDTO> GetCities(int limit, bool isActive = true);
		IEnumerable<StreetDTO> GetStreetsByCriteria(Guid cityGuid, string searchString, int limit, bool isActive = true);
		IEnumerable<HouseDTO> GetHousesFromStreetByCriteria(Guid streetGuid, string streetDistrict, int? limit = 50, bool isActive = true);
		IEnumerable<HouseDTO> GetHousesFromCityByCriteria(Guid cityGuid, string searchString, int? limit = 50, bool isActive = true);
		Task<PointDTO> GetCoordinatesByGeoCoderAsync(string address);
	}
}
