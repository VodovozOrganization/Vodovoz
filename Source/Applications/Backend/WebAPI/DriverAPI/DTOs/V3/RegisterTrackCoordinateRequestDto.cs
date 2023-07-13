using DriverAPI.Library.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DriverAPI.DTOs.V3
{
	public class RegisterTrackCoordinateRequestDto
	{
		/// <summary>
		/// Номер маршрутного листа
		/// </summary>
		[Required]
		public int RouteListId { get; set; }

		/// <summary>
		/// Координаты трека
		/// </summary>
		[Required]
		public IEnumerable<TrackCoordinateDto> TrackList { get; set; }
	}
}
