using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Доступы пользователя
	/// </summary>
	public class UserAccessDto
	{
		/// <summary>
		/// Доступ ЛК
		/// </summary>
		[JsonPropertyName("web")]
		public bool? IsWebAvailable { get; set; }

		/// <summary>
		/// Доступ API
		/// </summary>
		[JsonPropertyName("api")]
		public bool? IsApiAvailable { get; set; }

		/// <summary>
		/// Доступ МП
		/// </summary>
		[JsonPropertyName("mobile")]
		public bool? IsMobileAvailable { get; set; }
	}
}
