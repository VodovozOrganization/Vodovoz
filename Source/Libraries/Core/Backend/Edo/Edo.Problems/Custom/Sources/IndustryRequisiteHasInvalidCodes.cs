using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	public class IndustryRequisiteHasInvalidCodes : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.IndustryRequisiteHasInvalidCodes";
		public override string Message => "Обнаружены не валидные коды при проверке в честном знаке";
		public override string Description => "Проверяет все ли коды в задаче валидны и могут быть переданы в чек";
		public override string Recommendation => "Необходимо решение пользователя по кодам: " +
			"договоренность с клиентом и изменение или отмена заказа";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
