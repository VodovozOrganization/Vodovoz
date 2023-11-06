using System;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;

namespace Fias.Client.Loaders
{
	public interface IHousesDataLoader
	{
		event Action HousesLoaded;

		/// <summary>
		///     Initialize loading geodata from service
		/// </summary>
		void LoadHouses(string searchString = null, Guid? streetGuid = null, Guid? cityGuid = null);

		HouseDTO[] GetHouses();

		Task<PointDTO> GetCoordinatesByGeocoderAsync(string address, CancellationToken cancellationToken);
	}
}
