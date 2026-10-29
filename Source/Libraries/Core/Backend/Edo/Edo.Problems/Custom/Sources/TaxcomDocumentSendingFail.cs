using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Ошибка отправки документа в Такском, например, когда сервер вернул ошибку (4xx/5xx)
	/// </summary>
	public class TaxcomDocumentSendingFail : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.TaxcomDocumentSendingFail";
		public override string Message => "Ошибка при попытке отправки документ в Такском";
		public override string Description => "Не удалось отправить документ в Такском";
		public override string Recommendation => "Проверьте доступность сервера Такском, наличие подписи и повторите отправку";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
