using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{

	public class TaxcomSendDocumentProblemSource : EdoTaskProblemExceptionSource
	{
		public override string Name => nameof(TaxcomSendDocumentProblemSource);
		public override string Description => "Не удалось отправить документ в Такском";
		public override string Recommendation => "Проверьте доступность сервера Такском, наличие подписи и повторите отправку";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
