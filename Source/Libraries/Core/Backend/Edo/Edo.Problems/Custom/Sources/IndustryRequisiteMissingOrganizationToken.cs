using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class IndustryRequisiteMissingOrganizationToken : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.IndustryRequisiteMissingOrganizationToken";
		public override string Message => "Отсутствует токен организации для доступа в честный знак";
		public override string Description => "Проверяет установлен ли токен доступа в честный знак для организации";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
