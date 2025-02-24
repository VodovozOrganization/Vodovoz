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

	public class IndustryRequisiteCheckApiError : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.IndustryRequisiteCheckApiError";
		public override string Message => "Ошибка при проверки кодов в честном знаке";
		public override string Description => "Проверяет успешно ли прошел вызов проверки кодов в честном знаке";
		public override string Recommendation => "Обратитесь за технической поддержкой";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}

	public class IndustryRequisiteHasInvalidCodes : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.IndustryRequisiteHasInvalidCodes";
		public override string Message => "Обнаружены не валидные коды при проверке в честном знаке";
		public override string Description => "Проверяет все ли коды в задаче валидны и могут быть переданы в чек";
		public override string Recommendation => "Необходимо решение пользователя по кодам: " +
			"договоренность с клиентом и изменение или отмена заказа";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}

	public class ReceiptPrepareMaxAttemptsReached : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.ReceiptPrepareMaxAttemptsReached";
		public override string Message => "Достигнуто предельное количество попыток подготовки чека";
		public override string Description => "Проверяет количество попыток подготовки чека";
		public override string Recommendation => "Проверьте коды товаров в задаче и перезапустите вручную после исправления";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
