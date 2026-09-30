using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Ошибка отправки чека: не удалось установить SSL-соединение с МодульКассой.
	/// </summary>
	public class ReceiptSendSslError : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptSendSslError";
		public override string Message => "Ошибка SSL при обращении к кассе";
		public override string Description => "Возникает при отправке или проверке чека, если не удалось установить SSL-соединение с МодульКассой";
		public override string Recommendation => "Проверьте доступность сервиса МодульКассы и SSL-сертификаты. При необходимости обратитесь в техподдержку";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
