using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Тип упаковки
	/// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GeneralPackageType
	{
		/// <summary>
		/// Транспортная упаковка
		/// </summary>
		Box,
		/// <summary>
		/// Групповая упаковка
		/// </summary>
		Group,
		/// <summary>
		/// Индтивидуальная упаковка
		/// </summary>
		Unit
	}
}
