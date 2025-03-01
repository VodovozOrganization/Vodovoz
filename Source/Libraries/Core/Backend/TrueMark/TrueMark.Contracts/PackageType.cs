using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Уровень вложенности типа упаковки упаковки
	/// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PackageType
	{
		/// <summary>
		/// Уровень 1
		/// </summary>
		Level1,
		/// <summary>
		/// Уровень 2
		/// </summary>
		Level2,
		/// <summary>
		/// Уровень 3
		/// </summary>
		Level3,
		/// <summary>
		/// Уровень 4
		/// </summary>
		Level4,
		/// <summary>
		/// Уровень 5
		/// </summary>
		Level5
	}
}
