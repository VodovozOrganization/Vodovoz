using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto.Edo
{
	/// <summary>
	/// Цель покупки воды
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum WaterPurposeOfPurchase
	{
		/// <summary>
		/// Собственные нужды
		/// </summary>
		OwnNeeds,
		/// <summary>
		/// Перепродажа
		/// </summary>
		Resale
	}
}
