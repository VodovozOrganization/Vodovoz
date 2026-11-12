using System;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Ближайшее сгорание бонусов
	/// </summary>
	public sealed class MindboxBalanceNearestExpiration
	{
		[JsonPropertyName("total")]
		public decimal Total { get; set; }

		[JsonPropertyName("date")]
		public DateTime? Date { get; set; }
	}
}
