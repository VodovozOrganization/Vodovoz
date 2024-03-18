using System;
using Fias.Search.DTO;

namespace Fias.Client.Loaders
{
	public interface IStreetsDataLoader
	{
		/// <summary>
		///     Delay before query was executed (in millisecond)
		/// </summary>
		int Delay { get; set; }

		event Action StreetLoaded;

		/// <summary>
		///     Initialize loading geodata from service
		/// </summary>
		void LoadStreets(Guid? cityId, string searchString, int limit = 50);

		StreetDTO[] GetStreets();
	}
}
