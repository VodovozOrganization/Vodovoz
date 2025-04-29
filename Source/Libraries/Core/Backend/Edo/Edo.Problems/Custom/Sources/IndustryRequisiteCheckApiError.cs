using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class IndustryRequisiteCheckApiError : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.IndustryRequisiteCheckApiError";
		public override string Message => "Ошибка при проверки кодов в честном знаке";
		public override string Description => "Проверяет успешно ли прошел вызов проверки кодов в честном знаке";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
