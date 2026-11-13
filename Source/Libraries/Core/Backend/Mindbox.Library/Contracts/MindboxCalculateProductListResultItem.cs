using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Рассчитанная позиция списка товаров
	/// </summary>
	public sealed class MindboxCalculateProductListResultItem
	{
		/// <summary>
		/// Товар
		/// </summary>
		[JsonPropertyName("product")]
		public MindboxProduct Product { get; set; }

		/// <summary>
		/// Начисляемые бонусы
		/// </summary>
		[JsonPropertyName("earnBonusPoints")]
		public List<MindboxEarnBonusPoints> EarnBonusPoints { get; set; }
	}
}
