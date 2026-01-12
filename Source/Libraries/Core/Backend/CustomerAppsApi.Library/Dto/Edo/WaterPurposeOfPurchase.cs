using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace CustomerAppsApi.Library.Dto.Edo
{
	/// <summary>
	/// Цель покупки воды
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
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