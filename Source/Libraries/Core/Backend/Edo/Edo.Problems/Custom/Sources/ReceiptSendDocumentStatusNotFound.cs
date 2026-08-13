using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Ошибка отправки чека: документ не найден на стороне кассы при проверке статуса (HTTP 404).
	/// </summary>
	public class ReceiptSendDocumentStatusNotFound : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptSendDocumentStatusNotFound";
		public override string Message => "Не найден статус чека в кассе (HTTP 404 Not Found)";
		public override string Description => "Возникает при проверке статуса фискального документа, если МодульКасса отвечает HTTP 404 Not Found";
		public override string Recommendation => "Проверьте, был ли документ принят кассой. При необходимости переотправьте чек или обратитесь в техподдержку";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
