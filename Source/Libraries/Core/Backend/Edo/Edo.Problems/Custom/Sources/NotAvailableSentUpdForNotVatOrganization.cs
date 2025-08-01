using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class NotAvailableSentUpdForNotVatOrganization  : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.SentUpdNotAvailableForNotVatOrganization";
		public override string Message => "Нельзя отправлять УПД для организаций, работающих без НДС";
		public override string Description => "Проверяет как работает организация и если она без НДС, запрещает отправку УПД";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
