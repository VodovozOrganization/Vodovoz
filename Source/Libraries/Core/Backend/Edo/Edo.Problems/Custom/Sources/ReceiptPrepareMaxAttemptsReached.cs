using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class ReceiptPrepareMaxAttemptsReached : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptPrepareMaxAttemptsReached";
		public override string Message => "Достигнуто предельное количество попыток подготовки чека";
		public override string Description => "Проверяет количество попыток подготовки чека";
		public override string Recommendation => "Проверьте коды товаров в задаче и перезапустите вручную после исправления";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
