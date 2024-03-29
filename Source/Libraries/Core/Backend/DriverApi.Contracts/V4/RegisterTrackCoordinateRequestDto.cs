﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V4
{
	/// <summary>
	/// Запрос на регистрацию координат трека
	/// </summary>
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
