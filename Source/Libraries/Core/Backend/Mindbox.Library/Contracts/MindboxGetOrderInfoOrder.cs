using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Заказ в ответе GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoOrder
	{
		[JsonPropertyName("processingStatus")]
		public string ProcessingStatus { get; set; }

		[JsonPropertyName("lines")]
		public List<MindboxGetOrderInfoLine> Lines { get; set; }

		[JsonPropertyName("bonusPointsInfoPerBalanceType")]
		public List<MindboxGetOrderInfoBonusInfo> BonusPointsInfoPerBalanceType { get; set; }
	}
}
