using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Статус выполнения запроса, содержит код и сведения об ошибках
	/// </summary>
	public class StatusDto
	{
		/// <summary>
		/// Код ошибки, обязателен при любом результате выполнения запроса (200=успех)
		/// </summary>
		[JsonPropertyName("code")]
		public int StatusCode { get; set; }

		/// <summary>
		/// Массив ошибок, если ошибок не было, то параметр отсутствует
		/// </summary>
		[JsonPropertyName("errors")]
		public IEnumerable<ErrorDto> Errors { get; set; }
	}
}
