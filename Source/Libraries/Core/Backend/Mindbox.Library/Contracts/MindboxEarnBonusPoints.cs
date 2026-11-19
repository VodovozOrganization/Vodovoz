using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Начисляемые бонусы по балльному счёту
	/// </summary>
	public sealed class MindboxEarnBonusPoints
	{
		/// <summary>
		/// Количество начисляемых бонусов
		/// </summary>
		[JsonPropertyName("amount")]
		public decimal Amount { get; set; }

		/// <summary>
		/// Балльный счёт
		/// </summary>
		[JsonPropertyName("balance")]
		public MindboxBonusBalance Balance { get; set; }
	}
}
