using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class ReceiptSumInconsistent : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptSumInconsistent";
		public override string Message => "Сумма чека не совпадает с заказом";
		public override string Description => "Проверяет сумму подготовленного чека с суммой товаров заказа";
		public override string Recommendation => "Ошибка в расчете суммы, обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
