using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Ошибка отправки чека: сетевые проблемы (таймаут, отказ соединения, 502/504).
	/// </summary>
	public class ReceiptSendTransportError : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptSendTransportError";
		public override string Message => "Сетевая ошибка при обращении к кассе";
		public override string Description => "Возникает при отправке или проверке чека из-за таймаута, отказа соединения или ошибок шлюза (502/504)";
		public override string Recommendation => "Проверьте доступность сервиса МодульКассы и повторите отправку. При повторении обратитесь в техподдержку";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
