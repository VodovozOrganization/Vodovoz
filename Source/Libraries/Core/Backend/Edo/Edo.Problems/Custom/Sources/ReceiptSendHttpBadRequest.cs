using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Ошибка отправки чека: МодульКасса вернула HTTP 400 Bad Request.
	/// </summary>
	public class ReceiptSendHttpBadRequest : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptSendHttpBadRequest";
		public override string Message => "Касса отклонила чек (HTTP 400 Bad Request)";
		public override string Description => "Возникает при отправке фискального документа, если МодульКасса отвечает HTTP 400 Bad Request";
		public override string Recommendation => "Проверьте состав чека, маркировку и данные кассы. При необходимости обратитесь в техподдержку";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
