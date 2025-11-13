using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Тип контрагента
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PersonType
	{
		/// <summary>
		/// Физическое лицо
		/// </summary>
		Natural,
		/// <summary>
		/// Юридическое лицо
		/// </summary>
		Legal
	}
}
