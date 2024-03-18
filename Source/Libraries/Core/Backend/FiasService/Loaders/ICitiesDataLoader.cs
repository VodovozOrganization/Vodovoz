using System;
using Fias.Search.DTO;

namespace Fias.Client.Loaders
{
	public interface ICitiesDataLoader
	{
		/// <summary>
		///     Delay before query was executed (in millisecond)
		/// </summary>
		int Delay { get; set; }

		event Action CitiesLoaded;

		CityDTO GetCity(string cityName);

		/// <summary>
		///     Initialize loading geodata from service
		/// </summary>
		void LoadCities(string searchString, int limit = 50);

		CityDTO[] GetCities();
	}
}
