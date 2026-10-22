using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Данные физического лица
	/// </summary>
	public sealed class MindboxPhysicalFields
	{
		[JsonPropertyName("FirstName")]
		public string FirstName { get; set; }

		[JsonPropertyName("LastName")]
		public string LastName { get; set; }

		[JsonPropertyName("MiddleName")]
		public string MiddleName { get; set; }
	}
}
